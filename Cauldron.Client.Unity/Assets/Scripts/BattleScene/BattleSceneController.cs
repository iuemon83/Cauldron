using Assets.Scripts;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class BattleSceneController : MonoBehaviour
{
    public static BattleSceneController Instance;

    public PlayerId YouId => this.youPlayerController.PlayerId;

    [SerializeField]
    private HandCardController handCardPrefab = default;
    [SerializeField]
    private FieldCardController fieldCardPrefab = default;

    [SerializeField]
    private Canvas canvas = default;
    [SerializeField]
    private ConfirmDialogController confirmDialogController = default;
    [SerializeField]
    private ChoiceDialogController choiceDialogController = default;

    [SerializeField]
    private GameObject[] youHandSpaces = default;
    [SerializeField]
    private GameObject[] youFieldSpaces = default;

    [SerializeField]
    private PlayerController youPlayerController = default;

    [SerializeField]
    private GameObject[] opponentFieldSpaces = default;

    [SerializeField]
    private PlayerController opponentPlayerController = default;

    [SerializeField]
    private CardDetailController cardDetailController = default;

    [SerializeField]
    private Button choiceCardButton = default;
    [SerializeField]
    private TextMeshProUGUI numPicksText = default;
    [SerializeField]
    private TextMeshProUGUI numPicksLimitText = default;

    private readonly List<PlayerId> pickedPlayerIdList = new List<PlayerId>();
    private readonly List<CardId> pickedCardIdList = new List<CardId>();
    private readonly List<CardDefId> pickedCardDefIdList = new List<CardDefId>();

    private readonly Dictionary<CardId, HandCardController> handCardObjectsByCardId = new Dictionary<CardId, HandCardController>();
    private readonly Dictionary<CardId, FieldCardController> fieldCardControllersByCardId = new Dictionary<CardId, FieldCardController>();

    private readonly ConcurrentQueue<Func<UniTask>> updateViewActionQueue = new ConcurrentQueue<Func<UniTask>>();

    private Client client;

    private AskMessage askParams;

    private bool updating;

    private readonly List<IDisposable> disposableList = new List<IDisposable>();

    private GameContext currentGameContext;

    private FieldCardController attackCardController;

    private int NumPicks
    {
        get { return int.Parse(this.numPicksText.text); }
        set
        {
            this.numPicksText.text = value.ToString();
            if (value != 0
                && this.NumPicks >= this.NumPicksLimit)
            {
                this.numPicksText.color = Color.red;
            }
            else
            {
                this.numPicksText.color = Color.white;
            }
        }
    }
    private int NumPicksLimit
    {
        get { return int.Parse(this.numPicksLimitText.text); }
        set { this.numPicksLimitText.text = value.ToString(); }
    }

    private async void Start()
    {
        Instance = this;

        this.choiceCardButton.interactable = false;

        var holder = ConnectionHolder.Find();

        this.disposableList.AddRange(new[]
        {
            holder.Receiver.OnAddCard.Subscribe((a) => this.OnAddCard(a.gameContext, a.message)),
            holder.Receiver.OnAsk.Subscribe((a) => this.OnAsk(a)),
            holder.Receiver.OnBattle.Subscribe((a) => this.OnBattle(a.gameContext, a.message)),
            holder.Receiver.OnDamage.Subscribe((a) => this.OnDamage(a.gameContext, a.message)),
            holder.Receiver.OnModifyCard.Subscribe((a) => this.OnModifyCard(a.gameContext, a.message)),
            holder.Receiver.OnModifyPlayer.Subscribe((a) => this.OnModifyPlayer(a.gameContext, a.message)),
            holder.Receiver.OnModifyCounter.Subscribe((a) => this.OnModifyCounter(a.gameContext, a.message)),
            holder.Receiver.OnMoveCard.Subscribe((a) => this.OnMoveCard(a.gameContext, a.message)),
            holder.Receiver.OnExcludeCard.Subscribe((a) => this.OnExcludeCard(a.gameContext, a.message)),
            holder.Receiver.OnStartTurn.Subscribe((a) => this.OnStartTurn(a.gameContext, a.playerId)),
            holder.Receiver.OnEndGame.Subscribe(this.OnEndGame),
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

    private async void Update()
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

    public async UniTask PlayFromHand(HandCardController handCardController)
    {
        this.ResetAllMarks();
        await this.client.PlayFromHand(handCardController.CardId);
    }

    public async UniTask AttackToOpponentPlayerIfSelectedAttackCard()
    {
        if (this.attackCardController == null)
        {
            return;
        }

        await this.client.AttackToOpponentPlayer(this.attackCardController.CardId);

        // 攻撃後は選択済みのカードの選択を解除する
        this.UnSelectAttackCard();
    }

    public async void AttackToCardIfSelectedAttackCard(FieldCardController guardFieldCardController)
    {
        if (this.attackCardController == null)
        {
            // 攻撃元のカードが選択されていない
            return;
        }

        var attackCardId = this.attackCardController.CardId;
        var guardCardId = guardFieldCardController.CardId;

        // 攻撃する
        await this.client.Attack(attackCardId, guardCardId);

        // 攻撃後は選択済みのカードの選択を解除する
        this.UnSelectAttackCard();
    }

    public async void SetAttackCard(FieldCardController attackCardController)
    {
        var isSelected = this.attackCardController?.CardId == attackCardController.CardId;

        this.ResetAllMarks();

        if (!isSelected)
        {
            if (!fieldCardControllersByCardId.TryGetValue(attackCardController.CardId, out var fieldCardController))
            {
                return;
            }

            this.attackCardController = fieldCardController;
            this.attackCardController.VisibleAttackIcon(true);

            await this.MarkingAttackTargets();
        }
    }

    public async UniTask MarkingAttackTargets()
    {
        var targets = await this.client.ListAttackTargets(this.attackCardController.CardId);

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
        if (this.attackCardController != null)
        {
            this.attackCardController.VisibleAttackIcon(false);
            this.attackCardController = null;
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
    /// 降参ボタンのクイックイベント
    /// </summary>
    public void OnSurrenderButtonClick()
    {
        Debug.Log("click surrender Button!");

        this.ShowConfirmSurrenderDialog();
    }

    /// <summary>
    /// 選択完了ボタンのクリックイベント
    /// </summary>
    public async void OnPickedButtonClick()
    {
        this.choiceCardButton.interactable = false;

        var (isValid, picked) = this.ValidChoiceAnwser();
        if (!isValid)
        {
            Debug.Log("選択している対象が正しくない");
            this.choiceCardButton.interactable = true;
            return;
        }

        var result = await this.client.AnswerChoice(this.askParams.QuestionId, picked);
        if (result != GameMasterStatusCode.OK)
        {
            Debug.Log($"result: {result}");

            return;
        }

        this.NumPicks = 0;
        this.NumPicksLimit = 0;

        // リセット
        this.ResetAllMarks();
    }

    public (bool, ChoiceAnswer) ValidChoiceAnwser()
    {
        var picked = new ChoiceAnswer(
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

    public void ShowChoiceDialog()
    {
        var dialog = Instantiate(this.choiceDialogController);
        dialog.Init(this.askParams, async answer =>
        {
            if (answer.Count() > this.askParams.NumPicks)
            {
                // 選択した数が多い
                return;
            }

            var result = await this.client.AnswerChoice(this.askParams.QuestionId, answer);
            if (result != GameMasterStatusCode.OK)
            {
                Debug.Log($"result: {result}");

                return;
            }

            // リセット
            this.ResetAllMarks();
            Destroy(dialog.gameObject);
        });

        dialog.transform.SetParent(this.canvas.transform, false);
    }

    private async UniTask UpdateGameContext(GameContext gameContext)
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
            var removeHandIdList = this.handCardObjectsByCardId.Keys
                .Where(id => !youHands.Any(c => c.Id == id))
                .ToArray();
            foreach (var handCardId in removeHandIdList)
            {
                // なくなったものを削除する
                this.RemoveHandCardObj(handCardId);
            }
            foreach (var handIndex in Enumerable.Range(0, Mathf.Min(youHands.Length, 10)))
            {
                var handCard = youHands[handIndex];
                this.GetOrCreateHandCardObject(handCard.Id, handCard, handIndex);
            }

            var youFieldCards = publicInfo.Field;
            var removeFieldIdList = this.fieldCardControllersByCardId
                .Where(x => x.Value.Card.OwnerId == publicInfo.Id
                    && !youFieldCards.Any(c => c.Id == x.Key))
                .Select(x => x.Key)
                .ToArray();
            foreach (var fieldCardId in removeFieldIdList)
            {
                // なくなったものを削除する
                this.RemoveFieldCardObj(fieldCardId);
            }
            foreach (var fieldIndex in Enumerable.Range(0, Mathf.Min(youFieldCards.Length, 5)))
            {
                var fieldCard = youFieldCards[fieldIndex];
                var fieldCardObj = this.GetOrCreateFieldCardObject(fieldCard, publicInfo.Id, fieldIndex);
            }
        }

        var opponent = gameContext.Opponent;
        if (opponent != null)
        {
            this.opponentPlayerController.Set(opponent);

            var opponentFieldCards = opponent.Field;
            var removeFieldIdList = this.fieldCardControllersByCardId
                .Where(x => x.Value.Card.OwnerId == opponent.Id
                    && !opponentFieldCards.Any(c => c.Id == x.Key))
                .Select(x => x.Key)
                .ToArray();
            foreach (var fieldCardId in removeFieldIdList)
            {
                // なくなったものを削除する
                this.RemoveFieldCardObj(fieldCardId);
            }
            foreach (var fieldIndex in Enumerable.Range(0, Mathf.Min(opponentFieldCards.Length, 5)))
            {
                var fieldCard = opponentFieldCards[fieldIndex];

                var fieldCardObj = this.GetOrCreateFieldCardObject(fieldCard, opponent.Id, fieldIndex);
            }
        }

        this.currentGameContext = gameContext;

        if (gameContext.GameOver)
        {
            this.ShowEndGameDialog(gameContext.WinnerPlayerId);
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.3));
    }

    private HandCardController GetOrCreateHandCardObject(CardId cardId, Card card, int index)
    {
        if (!handCardObjectsByCardId.TryGetValue(cardId, out var controller))
        {
            this.RemoveCardObjectByCardId(cardId);

            controller = Instantiate(this.handCardPrefab);
            handCardObjectsByCardId.Add(cardId, controller);
        }

        controller.Init(card);

        controller.transform.SetParent(this.canvas.transform, false);
        controller.transform.position = this.youHandSpaces[index].transform.position;

        return controller;
    }

    private FieldCardController GetOrCreateFieldCardObject(Card card, PlayerId playerId, int index)
    {
        if (!fieldCardControllersByCardId.TryGetValue(card.Id, out var cardController))
        {
            this.RemoveCardObjectByCardId(card.Id);

            cardController = Instantiate(this.fieldCardPrefab);
            fieldCardControllersByCardId.Add(card.Id, cardController);
        }

        cardController.Init(card);

        cardController.transform.SetParent(this.canvas.transform, false);
        cardController.transform.position = playerId == this.YouId
            ? this.youFieldSpaces[index].transform.position
            : this.opponentFieldSpaces[index].transform.position;

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
        dialog.Init(title, message, ConfirmDialogController.DialogType.Message,
            onOkAction: async () =>
            {
                await this.client.LeaveGame();
                await Utility.LoadAsyncScene(SceneNames.ListGameScene);
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    public void ShowConfirmSurrenderDialog()
    {
        var title = "降参";
        var message = "降参しますか？";
        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.Confirm,
            onOkAction: async () =>
            {
                await this.client.Surrender();
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    void OnStartTurn(GameContext gameContext, PlayerId playerId)
    {
        Debug.Log($"OnStartTurn({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            Debug.Log($"ターン開始: {this.client.PlayerName}");

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

    void OnEndGame(GameContext gameContext)
    {
        Debug.Log($"OnEndGame({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            Debug.Log($"ゲーム終了: {this.client.PlayerName}");

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnAddCard(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage)
    {
        Debug.Log($"OnAddCard({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerName, cardName) = Utility.GetCardName(gameContext,
                addCardNotifyMessage.ToZone,
                addCardNotifyMessage.CardId);
            var playerName = Utility.GetPlayerName(gameContext, addCardNotifyMessage.ToZone.PlayerId);
            Debug.Log($"追加: {cardName}({ownerName}) to {addCardNotifyMessage.ToZone.ZoneName}({playerName})");

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnExcludeCard(GameContext gameContext, ExcludeCardNotifyMessage notify)
    {
        Debug.Log($"OnExcludeCard({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerName, cardName) = Utility.GetCardName(this.currentGameContext, notify.CardId);

            Debug.Log($"除外: {cardName}({ownerName})");

            if (this.fieldCardControllersByCardId.TryGetValue(notify.CardId, out var fieldCardController)
                && fieldCardController.Card.Type != CardType.Sorcery)
            {
                await fieldCardController.ExcludeEffect();
            }
            else if (this.handCardObjectsByCardId.TryGetValue(notify.CardId, out var handCardController))
            {
                await handCardController.ExcludeEffect();
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnMoveCard(GameContext gameContext, MoveCardNotifyMessage notify)
    {
        Debug.Log($"OnMoveCard({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerName, cardName) = Utility.GetCardName(gameContext, notify.ToZone, notify.CardId);
            var playerName = Utility.GetPlayerName(gameContext, notify.ToZone.PlayerId);

            Debug.Log($"移動: {cardName}({ownerName}) to {notify.ToZone.ZoneName}({playerName})");

            var cardId = notify.CardId;
            var targetPlayer = notify.ToZone.PlayerId == this.YouId
                ? this.youPlayerController
                : this.opponentPlayerController;

            switch (notify.ToZone.ZoneName)
            {
                case ZoneName.Cemetery:
                    {
                        if (this.fieldCardControllersByCardId.TryGetValue(cardId, out var fieldCardController)
                            && fieldCardController.Card.Type != CardType.Sorcery)
                        {
                            await fieldCardController.DestroyEffect();
                        }
                        else if (this.handCardObjectsByCardId.TryGetValue(cardId, out var handCardController))
                        {
                            await handCardController.DestroyEffect();
                        }
                        break;
                    }

                case ZoneName.Deck:
                    {
                        if (this.fieldCardControllersByCardId.TryGetValue(cardId, out var fieldCardController)
                            && fieldCardController.Card.Type != CardType.Sorcery)
                        {
                            await fieldCardController.BounceDeckEffect(targetPlayer);
                        }
                        else if (this.handCardObjectsByCardId.TryGetValue(cardId, out var handCardController))
                        {
                            await handCardController.BounceDeckEffect(targetPlayer);
                        }
                        break;
                    }

                case ZoneName.Hand:
                    {
                        if (this.fieldCardControllersByCardId.TryGetValue(cardId, out var fieldCardController)
                            && fieldCardController.Card.Type != CardType.Sorcery)
                        {
                            await fieldCardController.BounceHandEffect(targetPlayer);
                        }
                        break;
                    }

                case ZoneName.Field:
                    {
                        var card = notify.ToZone.PlayerId == this.YouId
                            ? gameContext.You.PublicPlayerInfo.Field.First(c => c.Id == cardId)
                            : gameContext.Opponent.Field.First(c => c.Id == cardId);

                        var fieldCard = this.GetOrCreateFieldCardObject(card, notify.ToZone.PlayerId, notify.Index);
                        await fieldCard.transform.DOScale(1.2f, 0);
                        await fieldCard.transform.DOScale(1f, 0.3f);
                        break;
                    }

                default:
                    break;
            }

            // 非公開領域へ移動した場合はgamecontextに含まれないのでここで削除する必要がある
            this.RemoveCardObjectByCardId(cardId);

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage)
    {
        Debug.Log($"OnModifyCard({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerName, cardName) = Utility.GetCardName(gameContext, modifyCardNotifyMessage.CardId);
            Debug.Log($"修整: {cardName}({ownerName})");

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage notify)
    {
        Debug.Log($"OnModifyPlayer({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var playerName = Utility.GetPlayerName(gameContext, notify.PlayerId);
            Debug.Log($"修整: {playerName}");

            static async UniTask HealOrDamageEffect(PlayerController playerController, int oldHp, int newHp)
            {
                if (playerController == null) return;

                var diffHp = newHp - oldHp;

                if (diffHp != 0)
                {
                    if (diffHp > 0)
                    {
                        await playerController.HealEffect(diffHp);
                    }
                    else
                    {
                        await playerController.DamageEffect(diffHp);
                    }
                }
            }

            if (this.youPlayerController.PlayerId == notify.PlayerId)
            {
                await HealOrDamageEffect(this.youPlayerController,
                    this.currentGameContext.You.PublicPlayerInfo.CurrentHp,
                    gameContext.You.PublicPlayerInfo.CurrentHp);
            }
            else if (this.opponentPlayerController.PlayerId == notify.PlayerId)
            {
                await HealOrDamageEffect(this.opponentPlayerController,
                    this.currentGameContext.Opponent.CurrentHp,
                    gameContext.Opponent.CurrentHp);
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnModifyCounter(GameContext gameContext, ModifyCounterNotifyMessage notify)
    {
        Debug.Log($"OnModifyCounter({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (notify.TargetCardId != default)
            {
                var (ownerName, cardName) = Utility.GetCardName(gameContext, notify.TargetCardId);
                Debug.Log($"カウンター: {notify.CounterName}({notify.NumCounters}) > {cardName}({ownerName})");
            }
            else if (notify.TargetPlayerId != default)
            {
                var playerName = Utility.GetPlayerName(gameContext, notify.TargetPlayerId);
                Debug.Log($"カウンター: {notify.CounterName}({notify.NumCounters}) > {playerName}");
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnBattle(GameContext gameContext, BattleNotifyMessage notify)
    {
        Debug.Log($"OnBattle({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerPlayerName, attackCardName) = Utility.GetCardName(gameContext, notify.AttackCardId);
            if (notify.GuardCardId == default)
            {
                var guardPlayerName = Utility.GetPlayerName(gameContext, notify.GuardPlayerId);
                Debug.Log($"戦闘: {attackCardName}({ownerPlayerName}) > {guardPlayerName}");
            }
            else
            {
                var (guardCardOwnerName, guardCardName) = Utility.GetCardName(gameContext, notify.GuardCardId);
                Debug.Log($"戦闘: {attackCardName}({ownerPlayerName}) > {guardCardName}({guardCardOwnerName})");
            }

            if (fieldCardControllersByCardId.TryGetValue(notify.AttackCardId, out var attackCard))
            {
                if (notify.GuardCardId == default)
                {
                    if (this.youPlayerController.PlayerId == notify.GuardPlayerId)
                    {
                        await attackCard.AttackEffect(this.youPlayerController);
                    }
                    else
                    {
                        await attackCard.AttackEffect(this.opponentPlayerController);
                    }
                }
                else
                {
                    if (fieldCardControllersByCardId.TryGetValue(notify.GuardCardId, out var guardCard))
                    {
                        await attackCard.AttackEffect(guardCard);
                    }
                }
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnDamage(GameContext gameContext, DamageNotifyMessage notify)
    {
        Debug.Log($"OnDamage({this.client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            switch (notify.Reason)
            {
                case DamageNotifyMessage.ReasonValue.DrawDeath:
                    {
                        var guardPlayerName = Utility.GetPlayerName(gameContext, notify.GuardPlayerId);
                        Debug.Log($"ダメージ: [{notify.Reason}] {guardPlayerName} {notify.Damage}");
                        break;
                    }

                case DamageNotifyMessage.ReasonValue.Attack:
                case DamageNotifyMessage.ReasonValue.Effect:
                    {
                        var (ownerPlayerName, cardName) = Utility.GetCardName(gameContext, notify.SourceCardId);
                        if (notify.GuardCardId == default)
                        {
                            var guardPlayerName = Utility.GetPlayerName(gameContext, notify.GuardPlayerId);
                            Debug.Log($"ダメージ: {cardName}({ownerPlayerName}) > {guardPlayerName} {notify.Damage}");
                        }
                        else
                        {
                            var (guardCardOwnerName, guardCardName) = Utility.GetCardName(gameContext, notify.GuardCardId);
                            Debug.Log($"ダメージ: {cardName}({ownerPlayerName}) > {guardCardName}({guardCardOwnerName}) {notify.Damage}");
                        }

                        break;
                    }
            }

            if (notify.GuardCardId == default)
            {
                if (this.youPlayerController.PlayerId == notify.GuardPlayerId)
                {
                    await this.youPlayerController.DamageEffect(notify.Damage);
                }
                else
                {
                    await this.opponentPlayerController.DamageEffect(notify.Damage);
                }
            }
            else
            {
                if (fieldCardControllersByCardId.TryGetValue(notify.GuardCardId, out var fieldCard))
                {
                    await fieldCard.DamageEffect(notify.Damage);
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

        this.askParams = askMessage;

        // ダイアログで選択させるか、フィールドから選択させるかの判定
        var choiceFromDialog = askMessage.ChoiceCandidates.CardDefList.Length != 0
            || askMessage.ChoiceCandidates.CardList
                .Any(x => x.Zone.ZoneName == ZoneName.Cemetery
                    || x.Zone.ZoneName == ZoneName.Deck
                    || x.Zone.ZoneName == ZoneName.CardPool);

        if (choiceFromDialog)
        {
            this.ShowChoiceDialog();
            return;
        }

        foreach (var player in new[] { this.youPlayerController, this.opponentPlayerController })
        {
            player.VisiblePickCandidateIcon(askMessage.ChoiceCandidates.PlayerIdList.Contains(player.PlayerId));
        }

        foreach (var card in askMessage.ChoiceCandidates.CardList)
        {
            if (fieldCardControllersByCardId.TryGetValue(card.Id, out var fieldCardController))
            {
                fieldCardController.VisiblePickCandidateIcon(true);
            }
        }

        this.choiceCardButton.interactable = true;
        this.NumPicks = 0;
        this.NumPicksLimit = this.askParams.NumPicks;
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
        this.NumPicks += 1;
    }

    public void UnPick(PlayerController playerController)
    {
        this.pickedPlayerIdList.Remove(playerController.PlayerId);
        playerController.ResetAllIcon();
        playerController.VisiblePickCandidateIcon(true);
        this.NumPicks -= 1;
    }

    public void Pick(CardController cardController)
    {
        this.pickedCardIdList.Add(cardController.CardId);
        cardController.ResetAllIcon();
        cardController.VisiblePickedIcon(true);
        this.NumPicks += 1;
    }

    public void UnPick(CardController cardController)
    {
        this.pickedCardIdList.Remove(cardController.CardId);
        cardController.ResetAllIcon();
        cardController.VisiblePickCandidateIcon(true);
        this.NumPicks -= 1;
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

        this.attackCardController = null;
        this.pickedCardDefIdList.Clear();
        this.pickedCardIdList.Clear();
        this.pickedPlayerIdList.Clear();
    }
}
