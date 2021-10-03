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
    [SerializeField]
    private GameObject pickUiGroup = default;

    private readonly List<PlayerId> pickedPlayerIdList = new List<PlayerId>();
    private readonly List<CardId> pickedCardIdList = new List<CardId>();
    private readonly List<CardDefId> pickedCardDefIdList = new List<CardDefId>();

    private readonly Dictionary<CardId, HandCardController> handCardObjectsByCardId = new Dictionary<CardId, HandCardController>();
    private readonly Dictionary<CardId, FieldCardController> fieldCardControllersByCardId = new Dictionary<CardId, FieldCardController>();

    private readonly ConcurrentQueue<Func<UniTask>> updateViewActionQueue = new ConcurrentQueue<Func<UniTask>>();

    private ConnectionHolder connectionHolder;

    private Client Client => this.connectionHolder.Client;

    private AskMessage askMessage;

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

        var holder = ConnectionHolder.Find();

        this.disposableList.AddRange(new[]
        {
            holder.Receiver.OnPlayCard.Subscribe((a) => this.OnPlayCard(a.gameContext, a.message)),
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

        this.connectionHolder = ConnectionHolder.Find();

        await this.Client.ReadyGame();
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
        await this.Client.PlayFromHand(handCardController.CardId);
    }

    public async UniTask AttackToOpponentPlayerIfSelectedAttackCard()
    {
        if (this.attackCardController == null)
        {
            return;
        }

        await this.Client.AttackToOpponentPlayer(this.attackCardController.CardId);

        // �U����͑I���ς݂̃J�[�h�̑I������������
        this.UnSelectAttackCard();
    }

    public async void AttackToCardIfSelectedAttackCard(FieldCardController guardFieldCardController)
    {
        if (this.attackCardController == null)
        {
            // �U�����̃J�[�h���I������Ă��Ȃ�
            return;
        }

        var attackCardId = this.attackCardController.CardId;
        var guardCardId = guardFieldCardController.CardId;

        // �U������
        await this.Client.Attack(attackCardId, guardCardId);

        // �U����͑I���ς݂̃J�[�h�̑I������������
        this.UnSelectAttackCard();
    }

    public async void ToggleAttackCard(FieldCardController attackCardController)
    {
        // �I�����[�h�ł͋@�\�����Ȃ�
        if (this.askMessage != null)
        {
            return;
        }

        var isSelected = this.attackCardController?.CardId == attackCardController.CardId;

        if (isSelected)
        {
            // ����
            this.attackCardController.VisibleAttackIcon(false);

            this.ResetAttackTargets();

            this.attackCardController = null;
        }
        else
        {
            this.ResetAllMarks();

            // �U���J�[�h�Ƃ��Ďw�肷��

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
        var targets = await this.Client.ListAttackTargets(this.attackCardController.CardId);

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
    /// �^�[���I���{�^���̃N�C�b�N�C�x���g
    /// </summary>
    public async void OnEndTurnButtonClick()
    {
        Debug.Log("click endturn Button!");

        this.ResetAllMarks();
        await this.Client.EndTurn();
    }

    /// <summary>
    /// �~�Q�{�^���̃N�C�b�N�C�x���g
    /// </summary>
    public void OnSurrenderButtonClick()
    {
        Debug.Log("click surrender Button!");

        this.ShowConfirmSurrenderDialog();
    }

    private async UniTask<bool> DoAnswer(ChoiceAnswer answer)
    {
        if (!this.ValidChoiceAnwser(answer))
        {
            return false;
        }

        var result = await this.Client.AnswerChoice(this.askMessage.QuestionId, answer);
        if (result != GameMasterStatusCode.OK)
        {
            Debug.Log($"result: {result}");

            return false;
        }

        // ���Z�b�g
        this.ResetAllMarks();

        this.NumPicks = 0;
        this.NumPicksLimit = 0;

        this.askMessage = null;

        return true;
    }

    private void ShowChoiceDialog()
    {
        var dialog = Instantiate(this.choiceDialogController);
        dialog.Init(this.askMessage, async answer =>
        {
            var result = await this.DoAnswer(answer);
            if (result)
            {
                Destroy(dialog.gameObject);
            }
        });

        dialog.transform.SetParent(this.canvas.transform, false);
    }

    /// <summary>
    /// �I�������{�^���̃N���b�N�C�x���g
    /// </summary>
    public async void OnPickedButtonClick()
    {
        this.choiceCardButton.interactable = false;

        var answer = this.Answer;

        var result = await this.DoAnswer(answer);
        if (result)
        {
            // ���Z�b�g
            this.pickUiGroup.SetActive(false);
            this.ResetAllMarks();
        }
        else
        {
            this.choiceCardButton.interactable = true;
        }
    }

    private ChoiceAnswer Answer => new ChoiceAnswer(
        this.pickedPlayerIdList.ToArray(),
        this.pickedCardIdList.ToArray(),
        this.pickedCardDefIdList.ToArray()
        );

    private bool ValidChoiceAnwser(ChoiceAnswer answer)
    {
        if (this.askMessage.NumPicks < answer.Count())
        {
            return false;
        }

        return true;
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
                // �Ȃ��Ȃ������̂��폜����
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
                // �Ȃ��Ȃ������̂��폜����
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
                // �Ȃ��Ȃ������̂��폜����
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
        var title = "�Q�[���I��";
        var message = winnerPlayerId == this.youPlayerController.PlayerId
            ? "���Ȃ��̏���!"
            : "���Ȃ��̕���...";
        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.Message,
            onOkAction: async () =>
            {
                await this.Client.LeaveGame();
                await Utility.LoadAsyncScene(SceneNames.ListGameScene);
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    public void ShowConfirmSurrenderDialog()
    {
        var title = "�~�Q";
        var message = "�~�Q���܂����H";
        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.Confirm,
            onOkAction: async () =>
            {
                await this.Client.Surrender();
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    void OnStartTurn(GameContext gameContext, PlayerId playerId)
    {
        Debug.Log($"OnStartTurn({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            Debug.Log($"�^�[���J�n: {this.Client.PlayerName}");

            if (this.youPlayerController.PlayerId == playerId)
            {
                this.youPlayerController.SetActiveTurn(true);
                this.opponentPlayerController.SetActiveTurn(false);
                await this.Client.StartTurn();
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
        Debug.Log($"OnEndGame({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            Debug.Log($"�Q�[���I��: {this.Client.PlayerName}");

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnPlayCard(GameContext gameContext, PlayCardNotifyMessage message)
    {
        Debug.Log($"OnPlayCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (!this.connectionHolder.CardPool.TryGetValue(message.CardDefId, out var carddef))
            {
                return;
            }

            var ownerName = Utility.GetPlayerName(gameContext, message.PlayerId);

            Debug.Log($"�v���C: {carddef.Name}({ownerName})");

            await this.ShowCardDetail(carddef);

            //await this.UpdateGameContext(gameContext);
        });
    }

    void OnAddCard(GameContext gameContext, AddCardNotifyMessage message)
    {
        Debug.Log($"OnAddCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerName, cardName) = Utility.GetCardName(gameContext,
                message.ToZone,
                message.CardId);
            var playerName = Utility.GetPlayerName(gameContext, message.ToZone.PlayerId);
            Debug.Log($"�ǉ�: {cardName}({ownerName}) to {message.ToZone.ZoneName}({playerName})");

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnExcludeCard(GameContext gameContext, ExcludeCardNotifyMessage message)
    {
        Debug.Log($"OnExcludeCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerName, cardName) = Utility.GetCardName(this.currentGameContext, message.CardId);

            Debug.Log($"���O: {cardName}({ownerName})");

            if (this.fieldCardControllersByCardId.TryGetValue(message.CardId, out var fieldCardController)
                && fieldCardController.Card.Type != CardType.Sorcery)
            {
                await fieldCardController.ExcludeEffect();
            }
            else if (this.handCardObjectsByCardId.TryGetValue(message.CardId, out var handCardController))
            {
                await handCardController.ExcludeEffect();
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnMoveCard(GameContext gameContext, MoveCardNotifyMessage message)
    {
        Debug.Log($"OnMoveCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerName, cardName) = Utility.GetCardName(gameContext, message.ToZone, message.CardId);
            var playerName = Utility.GetPlayerName(gameContext, message.ToZone.PlayerId);

            Debug.Log($"�ړ�: {cardName}({ownerName}) to {message.ToZone.ZoneName}({playerName})");

            var cardId = message.CardId;
            var targetPlayer = message.ToZone.PlayerId == this.YouId
                ? this.youPlayerController
                : this.opponentPlayerController;

            switch (message.ToZone.ZoneName)
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
                        var card = message.ToZone.PlayerId == this.YouId
                            ? gameContext.You.PublicPlayerInfo.Field.First(c => c.Id == cardId)
                            : gameContext.Opponent.Field.First(c => c.Id == cardId);

                        var fieldCard = this.GetOrCreateFieldCardObject(card, message.ToZone.PlayerId, message.Index);
                        await fieldCard.transform.DOScale(1.2f, 0);
                        await fieldCard.transform.DOScale(1f, 0.3f);
                        break;
                    }

                default:
                    break;
            }

            // ����J�̈�ֈړ������ꍇ��gamecontext�Ɋ܂܂�Ȃ��̂ł����ō폜����K�v������
            this.RemoveCardObjectByCardId(cardId);

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage message)
    {
        Debug.Log($"OnModifyCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerName, cardName) = Utility.GetCardName(gameContext, message.CardId);
            Debug.Log($"�C��: {cardName}({ownerName})");

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage message)
    {
        Debug.Log($"OnModifyPlayer({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var playerName = Utility.GetPlayerName(gameContext, message.PlayerId);
            Debug.Log($"�C��: {playerName}");

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

            if (this.youPlayerController.PlayerId == message.PlayerId)
            {
                await HealOrDamageEffect(this.youPlayerController,
                    this.currentGameContext.You.PublicPlayerInfo.CurrentHp,
                    gameContext.You.PublicPlayerInfo.CurrentHp);
            }
            else if (this.opponentPlayerController.PlayerId == message.PlayerId)
            {
                await HealOrDamageEffect(this.opponentPlayerController,
                    this.currentGameContext.Opponent.CurrentHp,
                    gameContext.Opponent.CurrentHp);
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnModifyCounter(GameContext gameContext, ModifyCounterNotifyMessage message)
    {
        Debug.Log($"OnModifyCounter({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (message.TargetCardId != default)
            {
                var (ownerName, cardName) = Utility.GetCardName(gameContext, message.TargetCardId);
                Debug.Log($"�J�E���^�[: {message.CounterName}({message.NumCounters}) > {cardName}({ownerName})");
            }
            else if (message.TargetPlayerId != default)
            {
                var playerName = Utility.GetPlayerName(gameContext, message.TargetPlayerId);
                Debug.Log($"�J�E���^�[: {message.CounterName}({message.NumCounters}) > {playerName}");
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnBattle(GameContext gameContext, BattleNotifyMessage message)
    {
        Debug.Log($"OnBattle({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerPlayerName, attackCardName) = Utility.GetCardName(gameContext, message.AttackCardId);
            if (message.GuardCardId == default)
            {
                var guardPlayerName = Utility.GetPlayerName(gameContext, message.GuardPlayerId);
                Debug.Log($"�퓬: {attackCardName}({ownerPlayerName}) > {guardPlayerName}");
            }
            else
            {
                var (guardCardOwnerName, guardCardName) = Utility.GetCardName(gameContext, message.GuardCardId);
                Debug.Log($"�퓬: {attackCardName}({ownerPlayerName}) > {guardCardName}({guardCardOwnerName})");
            }

            if (fieldCardControllersByCardId.TryGetValue(message.AttackCardId, out var attackCard))
            {
                if (message.GuardCardId == default)
                {
                    if (this.youPlayerController.PlayerId == message.GuardPlayerId)
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
                    if (fieldCardControllersByCardId.TryGetValue(message.GuardCardId, out var guardCard))
                    {
                        await attackCard.AttackEffect(guardCard);
                    }
                }
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnDamage(GameContext gameContext, DamageNotifyMessage message)
    {
        Debug.Log($"OnDamage({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            switch (message.Reason)
            {
                case DamageNotifyMessage.ReasonValue.DrawDeath:
                    {
                        var guardPlayerName = Utility.GetPlayerName(gameContext, message.GuardPlayerId);
                        Debug.Log($"�_���[�W: [{message.Reason}] {guardPlayerName} {message.Damage}");
                        break;
                    }

                case DamageNotifyMessage.ReasonValue.Attack:
                    {
                        var (ownerPlayerName, cardName) = Utility.GetCardName(gameContext, message.SourceCardId);
                        if (message.GuardCardId == default)
                        {
                            var guardPlayerName = Utility.GetPlayerName(gameContext, message.GuardPlayerId);
                            Debug.Log($"�_���[�W: {cardName}({ownerPlayerName}) > {guardPlayerName} {message.Damage}");
                        }
                        else
                        {
                            var (guardCardOwnerName, guardCardName) = Utility.GetCardName(gameContext, message.GuardCardId);
                            Debug.Log($"�_���[�W: {cardName}({ownerPlayerName}) > {guardCardName}({guardCardOwnerName}) {message.Damage}");
                        }

                        break;
                    }
                case DamageNotifyMessage.ReasonValue.Effect:
                    {
                        var card = Utility.GetCard(gameContext, message.SourceCardId);

                        if (card != null)
                        {
                            var ownerName = Utility.GetPlayerName(gameContext, card.OwnerId);

                            await this.ShowCardDetail(card);
                            Debug.Log($"�_���[�W: {card.Name}({ownerName}) {message.Damage}");
                        }

                        break;
                    }
            }

            if (message.GuardCardId == default)
            {
                if (this.youPlayerController.PlayerId == message.GuardPlayerId)
                {
                    await this.youPlayerController.DamageEffect(message.Damage);
                }
                else
                {
                    await this.opponentPlayerController.DamageEffect(message.Damage);
                }
            }
            else
            {
                if (fieldCardControllersByCardId.TryGetValue(message.GuardCardId, out var fieldCard))
                {
                    await fieldCard.DamageEffect(message.Damage);
                }
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnAsk(AskMessage message)
    {
        Debug.Log($"questionId={message.QuestionId}");

        this.pickedPlayerIdList.Clear();
        this.pickedCardIdList.Clear();
        this.pickedCardDefIdList.Clear();

        this.askMessage = message;

        // �_�C�A���O�őI�������邩�A�t�B�[���h����I�������邩�̔���
        var choiceFromDialog = message.ChoiceCandidates.CardDefList.Length != 0
            || message.ChoiceCandidates.CardList
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
            player.VisiblePickCandidateIcon(message.ChoiceCandidates.PlayerIdList.Contains(player.PlayerId));
        }

        foreach (var card in message.ChoiceCandidates.CardList)
        {
            if (fieldCardControllersByCardId.TryGetValue(card.Id, out var fieldCardController))
            {
                fieldCardController.VisiblePickCandidateIcon(true);
            }
        }

        this.pickUiGroup.SetActive(true);
        this.choiceCardButton.interactable = true;
        this.NumPicks = 0;
        this.NumPicksLimit = this.askMessage.NumPicks;
    }

    public async UniTask ShowCardDetail(Card card)
    {
        this.cardDetailController.SetCard(card);
        await cardDetailController.transform.DOScale(1.1f, 0);
        await cardDetailController.transform.DOScale(1f, 0.5f);
    }

    public async UniTask ShowCardDetail(CardDef cardDef)
    {
        this.cardDetailController.SetCardDef(cardDef);
        await cardDetailController.transform.DOScale(1.1f, 0);
        await cardDetailController.transform.DOScale(1f, 0.5f);
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
