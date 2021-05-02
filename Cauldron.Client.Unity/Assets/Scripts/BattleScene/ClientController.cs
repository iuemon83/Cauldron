using Assets.Scripts;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ClientController : MonoBehaviour
{
    public static Dictionary<CardId, GameObject> HandCardObjectsByCardId = new Dictionary<CardId, GameObject>();
    public static Dictionary<CardId, FieldCardController> FieldCardControllersByCardId = new Dictionary<CardId, FieldCardController>();

    public static ClientController Instance;

    public GameObject HandCardPrefab;
    public GameObject FieldCardPrefab;

    public Text YouName;
    public Text YouStatus;
    public Text OpponentName;
    public Text OpponentStatus;

    public GameObject[] YouHandSpaces;
    public GameObject[] YouFieldSpaces;
    public Text YouDeckText;
    public Text YouCemeteryText;

    public GameObject[] OpponentFieldSpaces;
    public Text OpponentHandText;
    public Text OpponentDeckText;
    public Text OpponentCemeteryText;

    public PlayerController OpponentPlayerController;

    public CardDetailController CardDetailController;

    public GameObject UiCanvas;

    public FieldCardController SelectedCardController { get; set; }

    public List<PlayerId> PickedPlayerIdList = new List<PlayerId>();
    public List<CardId> PickedCardIdList = new List<CardId>();
    public List<CardDefId> PickedCardDefIdList = new List<CardDefId>();

    private readonly ConcurrentQueue<GameContext> gameContextQueue = new ConcurrentQueue<GameContext>();
    private readonly float interval = 0.5f;

    private Client client;
    private float timeleft;

    private ChoiceCardsMessage askParams;

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

    void Update()
    {
        timeleft += Time.deltaTime;
        if (timeleft > interval)
        {
            timeleft = 0f;

            if (this.gameContextQueue.TryDequeue(out var gameContext))
            {
                this.UpdateGameContext(gameContext);
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
            this.OpponentPlayerController.AttackTargetIcon.SetActive(true);
        }

        foreach (var targetCardId in targets.Item2)
        {
            if (FieldCardControllersByCardId.TryGetValue(targetCardId, out var fieldCardController))
            {
                fieldCardController.AttackTargetIcon.SetActive(true);
            }
        }
    }

    public async ValueTask SelectCard(CardId cardId)
    {
        this.UnSelectCard();

        if (!FieldCardControllersByCardId.TryGetValue(cardId, out var fieldCardController))
        {
            return;
        }

        this.SelectedCardController = fieldCardController;
        this.SelectedCardController.SelectedIcon.SetActive(true);

        await this.MarkingAttackTargets();
    }

    public void UnSelectCard()
    {
        if (this.SelectedCardController != null)
        {
            this.SelectedCardController.SelectedIcon.SetActive(false);
            this.SelectedCardController = null;
        }

        this.ResetAttackTargets();
    }

    public void ResetAttackTargets()
    {
        this.OpponentPlayerController.AttackTargetIcon.SetActive(false);
        foreach (var fieldCardController in FieldCardControllersByCardId.Values)
        {
            fieldCardController.AttackTargetIcon.SetActive(false);
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
        this.OpponentPlayerController.PickCandidateIcon.SetActive(false);
        this.OpponentPlayerController.PickedIcon.SetActive(false);
        foreach (var fieldCardController in FieldCardControllersByCardId.Values)
        {
            fieldCardController.PickCandidateIcon.SetActive(false);
            fieldCardController.PickedIcon.SetActive(false);
        }
    }

    public (bool, ChoiceResult) ValidChoiceResult()
    {
        var picked = new ChoiceResult(
            this.PickedPlayerIdList.ToArray(),
            this.PickedCardIdList.ToArray(),
            this.PickedCardDefIdList.ToArray()
            );

        var numPicked = picked.PlayerIdList.Length + picked.CardIdList.Length + picked.CardDefIdList.Length;
        if (this.askParams.NumPicks < numPicked)
        {
            // 選択個数が多い
            return (false, default);
        }

        return (true, picked);
    }

    //private async void StartClient()
    //{
    //    //this.client = new Client(Config.ServerAddress, Config.PlayerName, this, Debug.Log, Debug.LogError);

    //    Utility.GameId = await this.client.OpenNewGame();
    //    Debug.Log("ゲームID: " + Utility.GameId);

    //    await this.client.EnterGame(Utility.GameId.Value);
    //    await this.client.ReadyGame();
    //}

    private void UpdateGameContext(GameContext gameContext)
    {
        if (gameContext == null)
        {
            return;
        }

        var you = gameContext.You;
        if (you != null)
        {
            var publicInfo = you.PublicPlayerInfo;
            this.YouName.text = publicInfo.Name;
            this.YouStatus.text = $"[{publicInfo.CurrentHp} / {publicInfo.MaxHp}] [{publicInfo.CurrentMp} / {publicInfo.MaxMp}]";
            this.YouDeckText.text = publicInfo.DeckCount.ToString();
            this.YouCemeteryText.text = publicInfo.Cemetery.Length.ToString();

            var youHands = you.Hands;
            foreach (var handIndex in Enumerable.Range(0, Mathf.Min(youHands.Length, 10)))
            {
                var handCard = youHands[handIndex];

                var handCardObj = this.GetOrCreateHandCardObject(handCard.Id, handCard);
                handCardObj.transform.SetParent(this.UiCanvas.transform, false);
                handCardObj.transform.position = this.YouHandSpaces[handIndex].transform.position;

                Debug.Log(handCardObj.transform.position);
            }

            var youFieldCards = publicInfo.Field;
            foreach (var fieldIndex in Enumerable.Range(0, Mathf.Min(youFieldCards.Length, 5)))
            {
                var fieldCard = youFieldCards[fieldIndex];

                var fieldCardObj = this.GetOrCreateFieldCardObject(fieldCard.Id, fieldCard);
                fieldCardObj.transform.SetParent(this.UiCanvas.transform, false);
                fieldCardObj.transform.position = this.YouFieldSpaces[fieldIndex].transform.position;
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
            this.OpponentPlayerController.PlayerId = opponent.Id;

            this.OpponentName.text = opponent.Name;
            this.OpponentStatus.text = $"[{opponent.CurrentHp} / {opponent.MaxHp}] [{opponent.CurrentMp} / {opponent.MaxMp}]";
            this.OpponentHandText.text = opponent.HandsCount.ToString();
            this.OpponentDeckText.text = opponent.DeckCount.ToString();
            this.OpponentCemeteryText.text = opponent.Cemetery.Length.ToString();

            var opponentFieldCards = opponent.Field;
            foreach (var fieldIndex in Enumerable.Range(0, Mathf.Min(opponentFieldCards.Length, 5)))
            {
                var fieldCard = opponentFieldCards[fieldIndex];

                var fieldCardObj = this.GetOrCreateFieldCardObject(fieldCard.Id, fieldCard);
                fieldCardObj.transform.SetParent(this.UiCanvas.transform, false);
                fieldCardObj.transform.position = this.OpponentFieldSpaces[fieldIndex].transform.position;
            }

            var opponentCemeteryCards = opponent.Cemetery;
            foreach (var cemeteryIndex in Enumerable.Range(0, opponentCemeteryCards.Length))
            {
                var cemeteryCard = opponentCemeteryCards[cemeteryIndex];
                RemoveCardObjectByCardId(cemeteryCard.Id);
            }
        }
    }

    private GameObject GetOrCreateHandCardObject(CardId cardId, Card card)
    {
        if (!HandCardObjectsByCardId.TryGetValue(cardId, out var cardObj))
        {
            cardObj = Instantiate(this.HandCardPrefab);
            HandCardObjectsByCardId.Add(cardId, cardObj);
        }

        var cardController = cardObj.GetComponent<CardController>();
        cardController.SetCard(card);

        return cardObj;
    }

    private FieldCardController GetOrCreateFieldCardObject(CardId cardId, Card card)
    {
        if (!FieldCardControllersByCardId.TryGetValue(cardId, out var cardController))
        {
            this.RemoveHandCardObj(cardId);

            cardController = Instantiate(this.FieldCardPrefab).GetComponent<FieldCardController>();
            FieldCardControllersByCardId.Add(cardId, cardController);
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
        if (HandCardObjectsByCardId.TryGetValue(cardId, out var cardObject))
        {
            HandCardObjectsByCardId.Remove(cardId);
            Destroy(cardObject);
        }
    }

    private void RemoveFieldCardObj(CardId cardId)
    {
        if (FieldCardControllersByCardId.TryGetValue(cardId, out var cardController))
        {
            FieldCardControllersByCardId.Remove(cardId);
            Destroy(cardController.gameObject);
        }
    }

    void OnReady(GameContext gameContext)
    {
        Debug.Log("OnReady");

        this.gameContextQueue.Enqueue(gameContext);
    }

    void OnStartGame(GameContext gameContext)
    {
        Debug.Log("ゲーム開始: " + this.client.PlayerName);
        this.gameContextQueue.Enqueue(gameContext);
    }

    void OnGameOver(GameContext gameContext)
    {
        Debug.Log("OnGameOver");
        this.gameContextQueue.Enqueue(gameContext);
    }

    async void OnStartTurn(GameContext gameContext)
    {
        // 自分のターン
        Debug.Log("ターン開始: " + this.client.PlayerName);
        this.gameContextQueue.Enqueue(gameContext);

        await this.client.StartTurn();
    }

    void OnAddCard(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage)
    {
        var (ownerName, cardName) = Utility.GetCardName(gameContext, addCardNotifyMessage.ToZone, addCardNotifyMessage.CardId);
        var playerName = Utility.GetPlayerName(gameContext, addCardNotifyMessage.ToZone.PlayerId);

        Debug.Log($"追加: {cardName}({ownerName}) to {addCardNotifyMessage.ToZone.ZoneName}({playerName})");
        this.gameContextQueue.Enqueue(gameContext);
    }

    void OnMoveCard(GameContext gameContext, MoveCardNotifyMessage moveCardNotifyMessage)
    {
        var (ownerName, cardName) = Utility.GetCardName(gameContext, moveCardNotifyMessage.ToZone, moveCardNotifyMessage.CardId);
        var playerName = Utility.GetPlayerName(gameContext, moveCardNotifyMessage.ToZone.PlayerId);

        Debug.Log($"移動: {cardName}({ownerName}) to {moveCardNotifyMessage.ToZone.ZoneName}({playerName})");

        Debug.Log($"OnMoveCard({moveCardNotifyMessage.ToZone.PlayerId}): {this.client.PlayerName}");
        this.gameContextQueue.Enqueue(gameContext);
    }

    void OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage)
    {

        var (ownerName, cardName) = Utility.GetCardName(gameContext, modifyCardNotifyMessage.CardId);

        Debug.Log($"修整: {cardName}({ownerName})");
        this.gameContextQueue.Enqueue(gameContext);
    }

    void OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage modifyPlayerNotifyMessage)
    {
        var playerName = Utility.GetPlayerName(gameContext, modifyPlayerNotifyMessage.PlayerId);

        Debug.Log($"修整: {playerName}");
        this.gameContextQueue.Enqueue(gameContext);
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
        this.gameContextQueue.Enqueue(gameContext);
    }

    void OnChoiceCards(ChoiceCardsMessage choiceCardsMessage)
    {
        Debug.Log($"questionId={choiceCardsMessage.QuestionId}");

        this.PickedPlayerIdList.Clear();
        this.PickedCardIdList.Clear();
        this.PickedCardDefIdList.Clear();

        if (choiceCardsMessage.ChoiceCandidates.PlayerIdList.Any())
        {
            this.OpponentPlayerController.PickCandidateIcon.SetActive(true);
        }

        foreach (var card in choiceCardsMessage.ChoiceCandidates.CardList)
        {
            if (FieldCardControllersByCardId.TryGetValue(card.Id, out var fieldCardController))
            {
                fieldCardController.PickCandidateIcon.SetActive(true);
            }
        }

        this.askParams = choiceCardsMessage;
    }
}