using Assets.Scripts;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

public class BattleSceneController : MonoBehaviour
{
    public static BattleSceneController Instance;

    [SerializeField]
    private HandCardController handCardPrefab;
    [SerializeField]
    private FieldCardController fieldCardPrefab;

    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private ConfirmDialogController confirmDialogController;

    [SerializeField]
    private GameObject[] youHandSpaces;
    [SerializeField]
    private GameObject[] youFieldSpaces;

    [SerializeField]
    private PlayerController youPlayerController;

    [SerializeField]
    private GameObject[] opponentFieldSpaces;

    [SerializeField]
    private PlayerController opponentPlayerController;

    [SerializeField]
    private CardDetailController cardDetailController;

    public FieldCardController AttackCardController { get; set; }

    private readonly List<PlayerId> pickedPlayerIdList = new List<PlayerId>();
    private readonly List<CardId> pickedCardIdList = new List<CardId>();
    private readonly List<CardDefId> pickedCardDefIdList = new List<CardDefId>();

    private readonly Dictionary<CardId, HandCardController> handCardObjectsByCardId = new Dictionary<CardId, HandCardController>();
    private readonly Dictionary<CardId, FieldCardController> fieldCardControllersByCardId = new Dictionary<CardId, FieldCardController>();

    private readonly ConcurrentQueue<Func<Task>> updateViewActionQueue = new ConcurrentQueue<Func<Task>>();

    private Client client;

    private AskMessage askParams;

    private bool updating;

    private List<IDisposable> disposableList = new List<IDisposable>();

    async void Start()
    {
        Instance = this;

        var holder = ConnectionHolder.Find();

        this.disposableList.AddRange(new[]
        {
            holder.Receiver.OnAddCard.Subscribe((a) => this.OnAddCard(a.gameContext, a.addCardNotifyMessage)),
            holder.Receiver.OnAsk.Subscribe((a) => this.OnAsk(a)),
            holder.Receiver.OnDamage.Subscribe((a) => this.OnDamage(a.gameContext, a.damageNotifyMessage)),
            holder.Receiver.OnModifyCard.Subscribe((a) => this.OnModifyCard(a.gameContext, a.modifyCardNotifyMessage)),
            holder.Receiver.OnModifyPlayer.Subscribe((a) => this.OnModifyPlayer(a.gameContext, a.modifyPlayerNotifyMessage)),
            holder.Receiver.OnMoveCard.Subscribe((a) => this.OnMoveCard(a.gameContext, a.moveCardNotifyMessage)),
            holder.Receiver.OnReady.Subscribe((a) => this.OnReady(a)),
            holder.Receiver.OnStartGame.Subscribe((a) => this.OnStartGame(a)),
            holder.Receiver.OnStartTurn.Subscribe((a) => this.OnStartTurn(a.gameContext, a.playerId)),
        });

        this.client = ConnectionHolder.Find().Client;

        await this.client.ReadyGame();
    }

    private void OnDestroy()
    {
        foreach (var disposable in this.disposableList)
        {
            disposable.Dispose();
        }
    }

    async void Update()
    {
        if (!this.updating)
        {
            if (this.updateViewActionQueue.TryDequeue(out var updateViewAction))
            {
                this.updating = true;
                await updateViewAction();
                this.updating = false;
            }
        }
    }

    public async ValueTask PlayFromHand(HandCardController handCardController)
    {
        this.ResetAllMarks();
        await this.client.PlayFromHand(handCardController.CardId);
    }

    public async ValueTask AttackToOpponentPlayerIfSelectedAttackCard()
    {
        if (this.AttackCardController == null)
        {
            return;
        }

        await this.client.AttackToOpponentPlayer(this.AttackCardController.CardId);

        // 攻撃後は選択済みのカードの選択を解除する
        this.UnSelectAttackCard();
    }

    public async void AttackToCardIfSelectedAttackCard(FieldCardController guardFieldCardController)
    {
        if (this.AttackCardController == null)
        {
            // 攻撃元のカードが選択されていない
            return;
        }

        var attackCardId = this.AttackCardController.CardId;
        var guardCardId = guardFieldCardController.CardId;

        // 攻撃する
        await this.client.Attack(attackCardId, guardCardId);

        // 攻撃後は選択済みのカードの選択を解除する
        this.UnSelectAttackCard();
    }

    public async void SetAttackCard(FieldCardController attackCardController)
    {
        var isSelected = this.AttackCardController?.CardId == attackCardController.CardId;

        this.ResetAllMarks();

        if (!isSelected)
        {
            if (!fieldCardControllersByCardId.TryGetValue(attackCardController.CardId, out var fieldCardController))
            {
                return;
            }

            this.AttackCardController = fieldCardController;
            this.AttackCardController.VisibleAttackIcon(true);

            await this.MarkingAttackTargets();
        }
    }

    public async ValueTask MarkingAttackTargets()
    {
        var targets = await this.client.ListAttackTargets(this.AttackCardController.CardId);

        this.ResetAttackTargets();

        foreach (var targetPlayerId in targets.Item1)
        {
            foreach (var player in new[] { this.opponentPlayerController, this.youPlayerController })
            {
                player.VisibleAttackTargetIcon(player.PlayerId == targetPlayerId);
            }
        }

        foreach (var targetCardId in targets.Item2)
        {
            if (fieldCardControllersByCardId.TryGetValue(targetCardId, out var fieldCardController))
            {
                fieldCardController.VisibleAttackTargetIcon(true);
            }
        }
    }

    public void UnSelectAttackCard()
    {
        if (this.AttackCardController != null)
        {
            this.AttackCardController.VisibleAttackIcon(false);
            this.AttackCardController = null;
        }

        this.ResetAttackTargets();
    }

    public void ResetAttackTargets()
    {
        this.opponentPlayerController.VisibleAttackTargetIcon(false);
        foreach (var fieldCardController in fieldCardControllersByCardId.Values)
        {
            fieldCardController.VisibleAttackTargetIcon(false);
        }
    }

    /// <summary>
    /// ターン終了ボタンのクイックイベント
    /// </summary>
    public async void OnEndTurnButtonClick()
    {
        Debug.Log("click endturn Button!");

        this.ResetAllMarks();
        await this.client.EndTurn();
    }

    /// <summary>
    /// 選択完了ボタンのクリックイベント
    /// </summary>
    public async void OnPickedButtonClick()
    {
        Debug.Log("click picked button!");

        var (isValid, picked) = this.ValidChoiceResult();
        if (!isValid)
        {
            Debug.Log("選択している対象が正しくない");
            return;
        }

        var result = await this.client.AnswerChoice(this.askParams.QuestionId, picked);
        if (result != GameMasterStatusCode.OK)
        {
            Debug.Log($"result: {result}");

            return;
        }

        // リセット
        this.ResetAllMarks();
    }

    public (bool, ChoiceResult) ValidChoiceResult()
    {
        var picked = new ChoiceResult(
            this.pickedPlayerIdList.ToArray(),
            this.pickedCardIdList.ToArray(),
            this.pickedCardDefIdList.ToArray()
            );

        var numPicked = picked.PlayerIdList.Length + picked.CardIdList.Length + picked.CardDefIdList.Length;
        if (this.askParams.NumPicks < numPicked)
        {
            // 選択個数が多い
            return (false, default);
        }

        return (true, picked);
    }

    private async Task UpdateGameContext(GameContext gameContext)
    {
        if (gameContext == null)
        {
            return;
        }

        var you = gameContext.You;
        if (you != null)
        {
            var publicInfo = you.PublicPlayerInfo;
            this.youPlayerController.Set(publicInfo);

            var youHands = you.Hands;
            foreach (var handIndex in Enumerable.Range(0, Mathf.Min(youHands.Length, 10)))
            {
                var handCard = youHands[handIndex];

                var handCardObj = this.GetOrCreateHandCardObject(handCard.Id, handCard);
                handCardObj.transform.SetParent(this.canvas.transform, false);
                handCardObj.transform.position = this.youHandSpaces[handIndex].transform.position;

                Debug.Log(handCardObj.transform.position);
            }

            var youFieldCards = publicInfo.Field;
            foreach (var fieldIndex in Enumerable.Range(0, Mathf.Min(youFieldCards.Length, 5)))
            {
                var fieldCard = youFieldCards[fieldIndex];

                var fieldCardObj = this.GetOrCreateFieldCardObject(fieldCard.Id, fieldCard);
                fieldCardObj.transform.SetParent(this.canvas.transform, false);
                fieldCardObj.transform.position = this.youFieldSpaces[fieldIndex].transform.position;
            }

            var youCemeteryCards = publicInfo.Cemetery;
            foreach (var cemeteryIndex in Enumerable.Range(0, youCemeteryCards.Length))
            {
                var cemeteryCard = youCemeteryCards[cemeteryIndex];
                RemoveCardObjectByCardId(cemeteryCard.Id);
            }
        }

        var opponent = gameContext.Opponent;
        if (opponent != null)
        {
            this.opponentPlayerController.Set(opponent);

            var opponentFieldCards = opponent.Field;
            foreach (var fieldIndex in Enumerable.Range(0, Mathf.Min(opponentFieldCards.Length, 5)))
            {
                var fieldCard = opponentFieldCards[fieldIndex];

                var fieldCardObj = this.GetOrCreateFieldCardObject(fieldCard.Id, fieldCard);
                fieldCardObj.transform.SetParent(this.canvas.transform, false);
                fieldCardObj.transform.position = this.opponentFieldSpaces[fieldIndex].transform.position;
            }

            var opponentCemeteryCards = opponent.Cemetery;
            foreach (var cemeteryIndex in Enumerable.Range(0, opponentCemeteryCards.Length))
            {
                var cemeteryCard = opponentCemeteryCards[cemeteryIndex];
                RemoveCardObjectByCardId(cemeteryCard.Id);
            }
        }

        if (gameContext.GameOver)
        {
            this.ShowEndGameDialog(gameContext.WinnerPlayerId);
        }

        await Task.Delay(TimeSpan.FromSeconds(0.3));
    }

    private HandCardController GetOrCreateHandCardObject(CardId cardId, Card card)
    {
        if (!handCardObjectsByCardId.TryGetValue(cardId, out var cardObj))
        {
            cardObj = Instantiate(this.handCardPrefab);
            handCardObjectsByCardId.Add(cardId, cardObj);
        }

        var cardController = cardObj.GetComponent<CardController>();
        cardController.SetCard(card);

        return cardObj;
    }

    private FieldCardController GetOrCreateFieldCardObject(CardId cardId, Card card)
    {
        if (!fieldCardControllersByCardId.TryGetValue(cardId, out var cardController))
        {
            this.RemoveHandCardObj(cardId);

            cardController = Instantiate(this.fieldCardPrefab).GetComponent<FieldCardController>();
            fieldCardControllersByCardId.Add(cardId, cardController);
        }

        cardController.SetCard(card);

        return cardController;
    }

    private void RemoveCardObjectByCardId(CardId cardId)
    {
        this.RemoveHandCardObj(cardId);
        this.RemoveFieldCardObj(cardId);
    }

    private void RemoveHandCardObj(CardId cardId)
    {
        if (handCardObjectsByCardId.TryGetValue(cardId, out var cardController))
        {
            handCardObjectsByCardId.Remove(cardId);
            Destroy(cardController.gameObject);
        }
    }

    private void RemoveFieldCardObj(CardId cardId)
    {
        if (fieldCardControllersByCardId.TryGetValue(cardId, out var cardController))
        {
            fieldCardControllersByCardId.Remove(cardId);
            Destroy(cardController.gameObject);
        }
    }

    public void ShowEndGameDialog(PlayerId winnerPlayerId)
    {
        var title = "ゲーム終了";
        var message = winnerPlayerId == this.youPlayerController.PlayerId
            ? "あなたの勝ち!"
            : "あなたの負け...";
        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.Message);
        dialog.OnOkButtonClickAction = () =>
        {
            Utility.LoadAsyncScene(this, SceneNames.ListGameScene);
        };
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    void OnReady(GameContext gameContext)
    {
        Debug.Log("OnReady");

        this.updateViewActionQueue.Enqueue(async () => await this.UpdateGameContext(gameContext));
    }

    void OnStartGame(GameContext gameContext)
    {
        Debug.Log("ゲーム開始: " + this.client.PlayerName);
        this.updateViewActionQueue.Enqueue(async () => await this.UpdateGameContext(gameContext));
    }

    void OnStartTurn(GameContext gameContext, PlayerId playerId)
    {
        // 自分のターン
        Debug.Log("ターン開始: " + this.client.PlayerName);
        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (this.youPlayerController.PlayerId == playerId)
            {
                this.youPlayerController.SetActiveTurn(true);
                this.opponentPlayerController.SetActiveTurn(false);
                await this.client.StartTurn();
            }
            else
            {
                this.youPlayerController.SetActiveTurn(false);
                this.opponentPlayerController.SetActiveTurn(true);
            }
        });
    }

    void OnAddCard(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage)
    {
        var (ownerName, cardName) = Utility.GetCardName(gameContext, addCardNotifyMessage.ToZone, addCardNotifyMessage.CardId);
        var playerName = Utility.GetPlayerName(gameContext, addCardNotifyMessage.ToZone.PlayerId);

        Debug.Log($"追加: {cardName}({ownerName}) to {addCardNotifyMessage.ToZone.ZoneName}({playerName})");
        this.updateViewActionQueue.Enqueue(async () => await this.UpdateGameContext(gameContext));
    }

    void OnMoveCard(GameContext gameContext, MoveCardNotifyMessage moveCardNotifyMessage)
    {
        var (ownerName, cardName) = Utility.GetCardName(gameContext, moveCardNotifyMessage.ToZone, moveCardNotifyMessage.CardId);
        var playerName = Utility.GetPlayerName(gameContext, moveCardNotifyMessage.ToZone.PlayerId);

        Debug.Log($"移動: {cardName}({ownerName}) to {moveCardNotifyMessage.ToZone.ZoneName}({playerName})");

        Debug.Log($"OnMoveCard({moveCardNotifyMessage.ToZone.PlayerId}): {this.client.PlayerName}");
        this.updateViewActionQueue.Enqueue(async () => await this.UpdateGameContext(gameContext));
    }

    void OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage)
    {

        var (ownerName, cardName) = Utility.GetCardName(gameContext, modifyCardNotifyMessage.CardId);

        Debug.Log($"修整: {cardName}({ownerName})");
        this.updateViewActionQueue.Enqueue(async () => await this.UpdateGameContext(gameContext));
    }

    void OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage modifyPlayerNotifyMessage)
    {
        var playerName = Utility.GetPlayerName(gameContext, modifyPlayerNotifyMessage.PlayerId);

        Debug.Log($"修整: {playerName}");
        this.updateViewActionQueue.Enqueue(async () => await this.UpdateGameContext(gameContext));
    }

    void OnDamage(GameContext gameContext, DamageNotifyMessage damageNotifyMessage)
    {
        var (ownerPlayerName, cardName) = Utility.GetCardName(gameContext, damageNotifyMessage.SourceCardId);
        if (damageNotifyMessage.GuardCardId == default)
        {
            var guardPlayerName = Utility.GetPlayerName(gameContext, damageNotifyMessage.GuardPlayerId);
            Debug.Log($"ダメージ: {cardName}({ownerPlayerName}) > {guardPlayerName} {damageNotifyMessage.Damage}");
        }
        else
        {
            var (guardCardOwnerName, guardCardName) = Utility.GetCardName(gameContext, damageNotifyMessage.GuardCardId);
            Debug.Log($"ダメージ: {cardName}({ownerPlayerName}) > {guardCardName}({guardCardOwnerName}) {damageNotifyMessage.Damage}");
        }

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (damageNotifyMessage.GuardCardId == default)
            {
                if (this.youPlayerController.PlayerId == damageNotifyMessage.GuardPlayerId)
                {
                    this.youPlayerController.DamageEffect(damageNotifyMessage.Damage);
                }
                else
                {
                    this.opponentPlayerController.DamageEffect(damageNotifyMessage.Damage);
                }
            }
            else
            {
                if (fieldCardControllersByCardId.TryGetValue(damageNotifyMessage.GuardCardId, out var fieldCard))
                {
                    await fieldCard.DamageEffect(damageNotifyMessage.Damage);
                }
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnAsk(AskMessage askMessage)
    {
        Debug.Log($"questionId={askMessage.QuestionId}");

        this.pickedPlayerIdList.Clear();
        this.pickedCardIdList.Clear();
        this.pickedCardDefIdList.Clear();

        //TODO carddef の選択画面を実装する
        if (askMessage.ChoiceCandidates.CardDefList.Length != 0)
        {
            // とりあえずcarddef が候補ならランダムに選択する
            var randomPicked = askMessage.ChoiceCandidates.CardDefList
                .OrderBy(_ => Guid.NewGuid())
                .Take(askMessage.NumPicks)
                .Select(c => c.Id);
            this.pickedCardDefIdList.AddRange(randomPicked);
            this.askParams = askMessage;
            this.OnPickedButtonClick();
        }

        foreach (var playerId in askMessage.ChoiceCandidates.PlayerIdList)
        {
            foreach (var player in new[] { this.youPlayerController, this.opponentPlayerController })
            {
                player.VisiblePickCandidateIcon(player.PlayerId == playerId);
            }
        }

        foreach (var card in askMessage.ChoiceCandidates.CardList)
        {
            if (fieldCardControllersByCardId.TryGetValue(card.Id, out var fieldCardController))
            {
                fieldCardController.VisiblePickCandidateIcon(true);
            }
        }

        this.askParams = askMessage;
    }

    public void ShowCardDetail(Card card)
    {
        this.cardDetailController.SetCard(card);
    }

    public void Pick(PlayerController playerController)
    {
        this.pickedPlayerIdList.Add(playerController.PlayerId);
        playerController.ResetAllIcon();
        playerController.VisiblePickedIcon(true);
    }

    public void UnPick(PlayerController playerController)
    {
        this.pickedPlayerIdList.Remove(playerController.PlayerId);
        playerController.ResetAllIcon();
        playerController.VisiblePickCandidateIcon(true);
    }

    public void Pick(CardController cardController)
    {
        this.pickedCardIdList.Add(cardController.CardId);
        cardController.ResetAllIcon();
        cardController.VisiblePickedIcon(true);
    }

    public void UnPick(CardController cardController)
    {
        this.pickedCardIdList.Remove(cardController.CardId);
        cardController.ResetAllIcon();
        cardController.VisiblePickCandidateIcon(true);
    }

    public void ResetAllMarks()
    {
        foreach (var handController in this.handCardObjectsByCardId.Values)
        {
            handController.ResetAllIcon();
        }

        foreach (var fieldController in this.fieldCardControllersByCardId.Values)
        {
            fieldController.ResetAllIcon();
        }

        this.opponentPlayerController.ResetAllIcon();
        this.youPlayerController.ResetAllIcon();

        this.AttackCardController = null;
        this.pickedCardDefIdList.Clear();
        this.pickedCardIdList.Clear();
        this.pickedPlayerIdList.Clear();
    }
}