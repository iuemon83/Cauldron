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
    private GameObject handCardPrefab;
    [SerializeField]
    private GameObject fieldCardPrefab;

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

    [SerializeField]
    private GameObject uiCanvas;

    public FieldCardController SelectedCardController { get; set; }

    private readonly List<PlayerId> pickedPlayerIdList = new List<PlayerId>();
    private readonly List<CardId> pickedCardIdList = new List<CardId>();
    private readonly List<CardDefId> pickedCardDefIdList = new List<CardDefId>();

    private readonly Dictionary<CardId, GameObject> handCardObjectsByCardId = new Dictionary<CardId, GameObject>();
    private readonly Dictionary<CardId, FieldCardController> fieldCardControllersByCardId = new Dictionary<CardId, FieldCardController>();

    private readonly ConcurrentQueue<Func<Task>> updateViewActionQueue = new ConcurrentQueue<Func<Task>>();

    private Client client;

    private ChoiceCardsMessage askParams;

    private bool updating;

    async void Start()
    {
        Instance = this;

        var holder = ConnectionHolder.Find();
        holder.Receiver.OnAddCard.Subscribe((a) => this.OnAddCard(a.gameContext, a.addCardNotifyMessage));
        holder.Receiver.OnChoiceCards.Subscribe((a) => this.OnChoiceCards(a));
        holder.Receiver.OnDamage.Subscribe((a) => this.OnDamage(a.gameContext, a.damageNotifyMessage));
        holder.Receiver.OnGameOver.Subscribe((a) => this.OnGameOver(a));
        holder.Receiver.OnModifyCard.Subscribe((a) => this.OnModifyCard(a.gameContext, a.modifyCardNotifyMessage));
        holder.Receiver.OnModifyPlayer.Subscribe((a) => this.OnModifyPlayer(a.gameContext, a.modifyPlayerNotifyMessage));
        holder.Receiver.OnMoveCard.Subscribe((a) => this.OnMoveCard(a.gameContext, a.moveCardNotifyMessage));
        holder.Receiver.OnReady.Subscribe((a) => this.OnReady(a));
        holder.Receiver.OnStartGame.Subscribe((a) => this.OnStartGame(a));
        holder.Receiver.OnStartTurn.Subscribe((a) => this.OnStartTurn(a));

        this.client = ConnectionHolder.Find().Client;

        await this.client.ReadyGame();
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

    public async ValueTask PlayFromHand(CardId cardId)
    {
        await this.client.PlayFromHand(cardId);
    }

    public async ValueTask Attack(CardId attackCardId, CardId guardCardId)
    {
        await this.client.Attack(attackCardId, guardCardId);
    }

    public async ValueTask AttackToOpponentPlayer(CardId attackCardId)
    {
        await this.client.AttackToOpponentPlayer(attackCardId);
    }

    public async ValueTask MarkingAttackTargets()
    {
        var targets = await this.client.ListAttackTargets(this.SelectedCardController.CardId);

        this.ResetAttackTargets();

        foreach (var targetPlayerId in targets.Item1)
        {
            this.opponentPlayerController.SetAttackTarget(true);
        }

        foreach (var targetCardId in targets.Item2)
        {
            if (fieldCardControllersByCardId.TryGetValue(targetCardId, out var fieldCardController))
            {
                fieldCardController.SetAttackTarget(true);
            }
        }
    }

    public async ValueTask SelectCard(CardId cardId)
    {
        this.UnSelectCard();

        if (!fieldCardControllersByCardId.TryGetValue(cardId, out var fieldCardController))
        {
            return;
        }

        this.SelectedCardController = fieldCardController;
        this.SelectedCardController.SetSelect(true);

        await this.MarkingAttackTargets();
    }

    public void UnSelectCard()
    {
        if (this.SelectedCardController != null)
        {
            this.SelectedCardController.SetSelect(false);
            this.SelectedCardController = null;
        }

        this.ResetAttackTargets();
    }

    public void ResetAttackTargets()
    {
        this.opponentPlayerController.SetAttackTarget(false);
        foreach (var fieldCardController in fieldCardControllersByCardId.Values)
        {
            fieldCardController.SetAttackTarget(false);
        }
    }

    /// <summary>
    /// ターン終了ボタンのクイックイベント
    /// </summary>
    public async void OnEndTurnButtonClick()
    {
        Debug.Log("click endturn Button!");

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
        this.opponentPlayerController.SetPickeCandidate(false);
        this.opponentPlayerController.SetPicked(false);
        foreach (var fieldCardController in fieldCardControllersByCardId.Values)
        {
            fieldCardController.PickCandidateIcon.SetActive(false);
            fieldCardController.PickedIcon.SetActive(false);
        }
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
                handCardObj.transform.SetParent(this.uiCanvas.transform, false);
                handCardObj.transform.position = this.youHandSpaces[handIndex].transform.position;

                Debug.Log(handCardObj.transform.position);
            }

            var youFieldCards = publicInfo.Field;
            foreach (var fieldIndex in Enumerable.Range(0, Mathf.Min(youFieldCards.Length, 5)))
            {
                var fieldCard = youFieldCards[fieldIndex];

                var fieldCardObj = this.GetOrCreateFieldCardObject(fieldCard.Id, fieldCard);
                fieldCardObj.transform.SetParent(this.uiCanvas.transform, false);
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
                fieldCardObj.transform.SetParent(this.uiCanvas.transform, false);
                fieldCardObj.transform.position = this.opponentFieldSpaces[fieldIndex].transform.position;
            }

            var opponentCemeteryCards = opponent.Cemetery;
            foreach (var cemeteryIndex in Enumerable.Range(0, opponentCemeteryCards.Length))
            {
                var cemeteryCard = opponentCemeteryCards[cemeteryIndex];
                RemoveCardObjectByCardId(cemeteryCard.Id);
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(0.3));
    }

    private GameObject GetOrCreateHandCardObject(CardId cardId, Card card)
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
        if (handCardObjectsByCardId.TryGetValue(cardId, out var cardObject))
        {
            handCardObjectsByCardId.Remove(cardId);
            Destroy(cardObject);
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

    void OnGameOver(GameContext gameContext)
    {
        Debug.Log("OnGameOver");
        this.updateViewActionQueue.Enqueue(async () => await this.UpdateGameContext(gameContext));
    }

    async void OnStartTurn(GameContext gameContext)
    {
        // 自分のターン
        Debug.Log("ターン開始: " + this.client.PlayerName);
        this.updateViewActionQueue.Enqueue(async () => await this.UpdateGameContext(gameContext));

        await this.client.StartTurn();
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

    void OnChoiceCards(ChoiceCardsMessage choiceCardsMessage)
    {
        Debug.Log($"questionId={choiceCardsMessage.QuestionId}");

        this.pickedPlayerIdList.Clear();
        this.pickedCardIdList.Clear();
        this.pickedCardDefIdList.Clear();

        if (choiceCardsMessage.ChoiceCandidates.PlayerIdList.Any())
        {
            this.opponentPlayerController.SetPickeCandidate(true);
        }

        foreach (var card in choiceCardsMessage.ChoiceCandidates.CardList)
        {
            if (fieldCardControllersByCardId.TryGetValue(card.Id, out var fieldCardController))
            {
                fieldCardController.PickCandidateIcon.SetActive(true);
            }
        }

        this.askParams = choiceCardsMessage;
    }

    public void ShowCardDetail(Card card)
    {
        this.cardDetailController.SetCard(card);
    }

    public void Pick(PlayerId playerId)
    {
        this.pickedPlayerIdList.Add(playerId);
    }

    public void UnPick(PlayerId playerId)
    {
        this.pickedPlayerIdList.Remove(playerId);
    }

    public void Pick(CardId cardId)
    {
        this.pickedCardIdList.Add(cardId);
    }

    public void UnPick(CardId cardId)
    {
        this.pickedCardIdList.Remove(cardId);
    }
}