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
    private static readonly int MaxNumFields = 5;
    private static readonly int MaxNumHands = 10;

    public static BattleSceneController Instance;

    public Color YouColor => this.youColor;
    public Color OpponentColor => this.opponentColor;

    public PlayerId YouId => this.youPlayerController.PlayerId;

    [SerializeField]
    private Color youColor = default;
    [SerializeField]
    private Color opponentColor = default;

    [SerializeField]
    private HandCardController handCardPrefab = default;
    [SerializeField]
    private FieldCardController fieldCardPrefab = default;

    [SerializeField]
    private Canvas canvas = default;
    [SerializeField]
    private ConfirmDialogController confirmDialogPrefab = default;
    [SerializeField]
    private ChoiceDialogController choiceDialogPrefab = default;
    [SerializeField]
    private ActionLogViewController actionLogViewController = default;

    [SerializeField]
    private ReadonlyCardListViewController youCemeteryCardListViewController = default;
    [SerializeField]
    private ReadonlyCardListViewController opponentCemeteryCardListViewController = default;
    [SerializeField]
    private ReadonlyCardListViewController youExcludedCardListViewController = default;
    [SerializeField]
    private ReadonlyCardListViewController opponentExcludedCardListViewController = default;

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

    /// <summary>
    /// 場のカードを配置するためのコンテナ
    /// </summary>
    [SerializeField]
    private GameObject fieldCardsContainer = default;

    /// <summary>
    /// 手札カードを配置するためのコンテナ
    /// </summary>
    [SerializeField]
    private GameObject handCardsContainer = default;

    /// <summary>
    /// 選択中の手札カードを配置するためのコンテナ
    /// </summary>
    [SerializeField]
    private GameObject playTargetHandCardsContainer = default;

    [SerializeField]
    private CardBigDetailController cardDetailViewController = default;

    [SerializeField]
    private Button endTurnButton = default;
    [SerializeField]
    private Button surrenderButton = default;

    [SerializeField]
    private Button choiceCardButton = default;
    [SerializeField]
    private TextMeshProUGUI numPicksText = default;
    [SerializeField]
    private TextMeshProUGUI numPicksLimitText = default;
    [SerializeField]
    private GameObject pickUiGroup = default;

    [SerializeField]
    private AudioSource audioSource = default;

    private readonly Dictionary<CardId, HandCardController> handCardObjectsByCardId = new Dictionary<CardId, HandCardController>();
    private readonly Dictionary<CardId, FieldCardController> fieldCardControllersByCardId = new Dictionary<CardId, FieldCardController>();

    private readonly ConcurrentQueue<Func<UniTask>> updateViewActionQueue = new ConcurrentQueue<Func<UniTask>>();

    private ConnectionHolder connectionHolder;

    private Client Client => this.connectionHolder.Client;

    private bool updating;

    private readonly List<IDisposable> disposableList = new List<IDisposable>();

    private GameContext currentGameContext;

    private FieldCardController attackCardController;

    private HandCardController playTargetHand = default;

    private bool IsPlayable(CardId cardId) => this.currentGameContext?.You?.PlayableCards?.Contains(cardId) ?? false;

    private readonly ChoiceService choiceService = new ChoiceService();

    private int NumPicks
    {
        set
        {
            this.numPicksText.text = value.ToString();
            if (this.choiceService.IsLimit)
            {
                this.numPicksText.color = ChoiceService.LimitSelectedColor;
            }
            else
            {
                this.numPicksText.color = ChoiceService.NoLimitSelectedColor;
            }
        }
    }
    private int NumPicksLimit
    {
        set { this.numPicksLimitText.text = value.ToString(); }
    }

    private async void Start()
    {
        Instance = this;

        var holder = ConnectionHolder.Find();

        this.youCemeteryCardListViewController.InitAsYou("Cemetery");
        this.opponentCemeteryCardListViewController.InitAsOpponent("Cemetery");

        this.youExcludedCardListViewController.InitAsYou("Excluded");
        this.opponentExcludedCardListViewController.InitAsOpponent("Excluded");

        this.cardDetailController.Init(this.DisplayBigCardDetail);

        this.youPlayerController.Init(this.UnPick, this.Pick);
        this.opponentPlayerController.Init(this.UnPick, this.Pick);

        this.disposableList.AddRange(new[]
        {
            holder.Receiver.OnPlayCard.Subscribe((a) => this.OnPlayCard(a.gameContext, a.message)),
            holder.Receiver.OnAddCard.Subscribe((a) => this.OnAddCard(a.gameContext, a.message)),
            holder.Receiver.OnAsk.Subscribe((a) => this.OnAsk(a)),
            holder.Receiver.OnBattleStart.Subscribe((a) => this.OnBattleStart(a.gameContext, a.message)),
            holder.Receiver.OnBattleEnd.Subscribe((a) => this.OnBattleEnd(a.gameContext, a.message)),
            holder.Receiver.OnDamage.Subscribe((a) => this.OnDamage(a.gameContext, a.message)),
            holder.Receiver.OnHeal.Subscribe((a) => this.OnHeal(a.gameContext, a.message)),
            holder.Receiver.OnModifyCard.Subscribe((a) => this.OnModifyCard(a.gameContext, a.message)),
            holder.Receiver.OnModifyPlayer.Subscribe((a) => this.OnModifyPlayer(a.gameContext, a.message)),
            holder.Receiver.OnModifyCounter.Subscribe((a) => this.OnModifyCounter(a.gameContext, a.message)),
            holder.Receiver.OnMoveCard.Subscribe((a) => this.OnMoveCard(a.gameContext, a.message)),
            holder.Receiver.OnExcludeCard.Subscribe((a) => this.OnExcludeCard(a.gameContext, a.message)),
            holder.Receiver.OnStartTurn.Subscribe((a) => this.OnStartTurn(a.gameContext, a.message)),
            holder.Receiver.OnEndGame.Subscribe((a) => this.OnEndGame(a.gameContext, a.message)),
        });

        this.connectionHolder = ConnectionHolder.Find();

        this.youPlayerController.Set(this.Client.PlayerId);

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

    public void Pick(PlayerController controller)
    {
        this.choiceService.Pick(controller);
        this.NumPicks = this.choiceService.CurrentNumPicks;
    }

    public void UnPick(PlayerController controller)
    {
        this.choiceService.UnPick(controller);
        this.NumPicks = this.choiceService.CurrentNumPicks;
    }

    public void Pick(CardController controller)
    {
        this.choiceService.Pick(controller);
        this.NumPicks = this.choiceService.CurrentNumPicks;
    }

    public void UnPick(CardController controller)
    {
        this.choiceService.UnPick(controller);
        this.NumPicks = this.choiceService.CurrentNumPicks;
    }

    private void SetPlayTargetHand(HandCardController target)
    {
        // 選択モードでは機能させない
        if (this.choiceService.IsChoiceMode)
        {
            return;
        }

        if (!this.IsPlayable(target.CardId))
        {
            return;
        }

        if (this.playTargetHand == null)
        {
            // unityのnull判定はぶっ壊れてるのでnullを入れる
            this.playTargetHand = null;
        }

        var prevId = this.playTargetHand?.CardId;

        this.ResetAllMarks();

        if (prevId != target.CardId)
        {
            target.transform.SetParent(this.playTargetHandCardsContainer.transform, false);
            target.TogglePlayTarget();
            this.playTargetHand = target;
        }
    }

    public async UniTask PlayFromHand(HandCardController handCardController)
    {
        this.ResetAllMarks();
        await this.Client.PlayFromHand(handCardController.CardId);
    }

    public async UniTask AttackToOpponentPlayerIfSelectedAttackCard()
    {
        // 選択モードでは機能させない
        if (this.choiceService.IsChoiceMode)
        {
            return;
        }

        if (this.attackCardController == null)
        {
            return;
        }

        await this.Client.AttackToOpponentPlayer(this.attackCardController.CardId);

        // 攻撃後は選択済みのカードの選択を解除する
        this.UnSelectAttackCard();
    }

    public async void AttackToCardIfSelectedAttackCard(FieldCardController guardFieldCardController)
    {
        // 選択モードでは機能させない
        if (this.choiceService.IsChoiceMode)
        {
            return;
        }

        if (this.attackCardController == null)
        {
            // 攻撃元のカードが選択されていない
            return;
        }

        var attackCardId = this.attackCardController.CardId;
        var guardCardId = guardFieldCardController.CardId;

        // 攻撃する
        await this.Client.Attack(attackCardId, guardCardId);

        // 攻撃後は選択済みのカードの選択を解除する
        this.UnSelectAttackCard();
    }

    public void ToggleAttackCard(FieldCardController attackCardController)
    {
        // 選択モードでは機能させない
        if (this.choiceService.IsChoiceMode)
        {
            return;
        }

        var isSelected = this.attackCardController?.CardId == attackCardController.CardId;

        if (isSelected)
        {
            // 解除
            this.attackCardController.VisibleAttackIcon(false);

            this.ResetAttackTargets();

            this.attackCardController = null;
        }
        else
        {
            this.ResetAllMarks();

            // 攻撃カードとして指定する

            if (!fieldCardControllersByCardId.TryGetValue(attackCardController.CardId, out var fieldCardController))
            {
                return;
            }

            if (!this.CanAttack(fieldCardController.Card.Id))
            {
                return;
            }

            this.attackCardController = fieldCardController;
            this.attackCardController.VisibleAttackIcon(true);

            this.MarkingAttackTargets();
        }
    }

    private bool CanAttack(CardId cardid)
    {
        if (this.currentGameContext == null)
        {
            return false;
        }

        return (this.currentGameContext.You.PublicPlayerInfo.AttackableCardIdList.TryGetValue(cardid, out var youTargets)
            && youTargets.Any)
            ||
            (this.currentGameContext.Opponent.AttackableCardIdList.TryGetValue(cardid, out var opTargets)
                && opTargets.Any);
    }

    public void MarkingAttackTargets()
    {
        if (this.attackCardController == null)
        {
            return;
        }

        var targets = this.currentGameContext.You.PublicPlayerInfo.AttackableCardIdList[this.attackCardController.CardId];

        this.ResetAttackTargets();

        foreach (var targetPlayerId in targets.PlayerIdList)
        {
            foreach (var player in new[] { this.opponentPlayerController, this.youPlayerController })
            {
                player.VisibleAttackTargetIcon(player.PlayerId == targetPlayerId);
            }
        }

        foreach (var targetCardId in targets.CardIdList)
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
    /// 自分の墓地ビューボタンのクリックイベント
    /// </summary>
    public void OnYouCemeteryButtonClick()
    {
        this.opponentCemeteryCardListViewController.Hidden();
        this.youExcludedCardListViewController.Hidden();
        this.opponentExcludedCardListViewController.Hidden();
        this.youCemeteryCardListViewController.ToggleDisplay();
    }

    /// <summary>
    /// 相手の墓地ビューボタンのクリックイベント
    /// </summary>
    public void OnOpponentCemeteryButtonClick()
    {
        this.youCemeteryCardListViewController.Hidden();
        this.youExcludedCardListViewController.Hidden();
        this.opponentExcludedCardListViewController.Hidden();
        this.opponentCemeteryCardListViewController.ToggleDisplay();
    }

    /// <summary>
    /// 自分の除外ビューボタンのクリックイベント
    /// </summary>
    public void OnYouExcludedButtonClick()
    {
        this.youCemeteryCardListViewController.Hidden();
        this.opponentCemeteryCardListViewController.Hidden();
        this.opponentExcludedCardListViewController.Hidden();
        this.youExcludedCardListViewController.ToggleDisplay();
    }

    /// <summary>
    /// 相手の除外ビューボタンのクリックイベント
    /// </summary>
    public void OnOpponentExcludedButtonClick()
    {
        this.youCemeteryCardListViewController.Hidden();
        this.opponentCemeteryCardListViewController.Hidden();
        this.youExcludedCardListViewController.Hidden();
        this.opponentExcludedCardListViewController.ToggleDisplay();
    }

    /// <summary>
    /// ターン終了ボタンのクリックイベント
    /// </summary>
    public async void OnEndTurnButtonClick()
    {
        Debug.Log("click endturn Button!");

        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

        this.ResetAllMarks();

        await UniTask.Delay(TimeSpan.FromSeconds(1));

        await this.Client.EndTurn();
    }

    /// <summary>
    /// 降参ボタンのクリックイベント
    /// </summary>
    public void OnSurrenderButtonClick()
    {
        Debug.Log("click surrender Button!");

        this.ShowConfirmSurrenderDialog();
    }

    private async UniTask AddActionLog(ActionLog actionLog, Card effectOwnerCard = default, CardEffectId? effectId = default)
    {
        await this.actionLogViewController.AddLog(actionLog, effectOwnerCard, effectId);
    }

    private async UniTask<bool> DoAnswer(ChoiceAnswer answer)
    {
        if (!this.choiceService.ValidChoiceAnwser(answer))
        {
            return false;
        }

        var result = await this.Client.AnswerChoice(this.choiceService.AskMessage.QuestionId, answer);
        if (result != GameMasterStatusCode.OK)
        {
            Debug.Log($"result: {result}");

            return false;
        }

        // リセット
        this.FinishChoiceMode();

        return true;
    }

    private void ShowChoiceDialog()
    {
        var dialog = Instantiate(this.choiceDialogPrefab);
        dialog.Init(this.choiceService.AskMessage, async answer =>
        {
            this.PlayAudio(SeAudioCache.SeAudioType.Ok);

            var result = await this.DoAnswer(answer);
            if (result)
            {
                Destroy(dialog.gameObject);
            }
        });

        dialog.transform.SetParent(this.canvas.transform, false);
    }

    /// <summary>
    /// 選択完了ボタンのクリックイベント
    /// </summary>
    public async void OnPickedButtonClick()
    {
        this.choiceCardButton.interactable = false;

        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

        var answer = this.choiceService.Answer;

        var result = await this.DoAnswer(answer);
        if (!result)
        {
            this.choiceCardButton.interactable = true;
        }
    }

    private void StartChoiceMode(AskMessage askMessage)
    {
        // カード選択に必要なボタンだけを有効にする
        this.ResetAllMarks();
        this.endTurnButton.interactable = false;
        this.surrenderButton.interactable = false;

        this.pickUiGroup.SetActive(true);
        this.choiceCardButton.interactable = true;
        this.NumPicks = 0;

        this.choiceService.Init(askMessage);
        this.NumPicksLimit = this.choiceService.LimitNumPicks;
    }

    private void FinishChoiceMode()
    {
        this.NumPicksLimit = 0;
        this.NumPicks = 0;
        this.choiceCardButton.interactable = false;
        this.pickUiGroup.SetActive(false);

        this.endTurnButton.interactable = true;
        this.surrenderButton.interactable = true;
        this.ResetAllMarks();
    }


    private async UniTask UpdateGameContext(GameContext gameContext)
    {
        if (gameContext == null)
        {
            return;
        }

        this.currentGameContext = gameContext;

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
            foreach (var handIndex in Enumerable.Range(0, Mathf.Min(youHands.Length, MaxNumHands)))
            {
                var handCard = youHands[handIndex];
                this.GetOrCreateHandCardObject(handCard.Id, handCard, handIndex);
            }

            var youFieldCards = publicInfo.Field;
            var removeFieldIdList = this.fieldCardControllersByCardId
                .Where(x => x.Value.Card.OwnerId == publicInfo.Id
                    && !youFieldCards.Any(c => c?.Id == x.Key))
                .Select(x => x.Key)
                .ToArray();
            foreach (var fieldCardId in removeFieldIdList)
            {
                // なくなったものを削除する
                this.RemoveFieldCardObj(fieldCardId);
            }
            foreach (var fieldIndex in Enumerable.Range(0, Mathf.Min(youFieldCards.Length, MaxNumFields)))
            {
                var fieldCard = youFieldCards[fieldIndex];
                if (fieldCard != null)
                {
                    this.GetOrCreateFieldCardObject(fieldCard, publicInfo.Id, fieldIndex);
                }
            }
        }

        var opponent = gameContext.Opponent;
        if (opponent != null)
        {
            this.opponentPlayerController.Set(opponent);

            var opponentFieldCards = opponent.Field;
            var removeFieldIdList = this.fieldCardControllersByCardId
                .Where(x => x.Value.Card.OwnerId == opponent.Id
                    && !opponentFieldCards.Any(c => c?.Id == x.Key))
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

                if (fieldCard != null)
                {
                    this.GetOrCreateFieldCardObject(fieldCard, opponent.Id, fieldIndex);
                }
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.3));
    }

    private async UniTask UpdateGameContextSimple(GameContext gameContext)
    {
        if (gameContext == null)
        {
            return;
        }

        this.currentGameContext = gameContext;

        var you = gameContext.You;
        if (you != null)
        {
            var publicInfo = you.PublicPlayerInfo;
            this.youPlayerController.Set(publicInfo);
        }

        var opponent = gameContext.Opponent;
        if (opponent != null)
        {
            this.opponentPlayerController.Set(opponent);

        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.3));
    }

    private HandCardController GetOrCreateHandCardObject(CardId cardId, Card card, int index)
    {
        if (!handCardObjectsByCardId.TryGetValue(cardId, out var controller))
        {
            this.RemoveCardObjectByCardId(cardId);

            controller = Instantiate(this.handCardPrefab, this.handCardsContainer.transform);
            handCardObjectsByCardId.Add(cardId, controller);
        }

        controller.Init(card, this.DisplaySmallCardDetailSimple,
            this.UnPick,
            this.Pick,
            this.SetPlayTargetHand
            );

        controller.transform.position = this.youHandSpaces[index].transform.position;

        controller.SetCanPlay(this.IsPlayable(cardId));

        return controller;
    }

    private FieldCardController GetOrCreateFieldCardObject(Card card, PlayerId playerId, int index)
    {
        if (!fieldCardControllersByCardId.TryGetValue(card.Id, out var cardController))
        {
            this.RemoveCardObjectByCardId(card.Id);

            cardController = Instantiate(this.fieldCardPrefab, this.fieldCardsContainer.transform);
            fieldCardControllersByCardId.Add(card.Id, cardController);
        }

        cardController.Init(card, this.DisplaySmallCardDetailSimple,
            this.UnPick,
            this.Pick
            );

        cardController.transform.position = playerId == this.YouId
            ? this.youFieldSpaces[index].transform.position
            : this.opponentFieldSpaces[index].transform.position;

        var canAttack = this.currentGameContext.ActivePlayerId == card.OwnerId
            && this.CanAttack(card.Id);
        cardController.SetCanAttack(canAttack);

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

    private void AddCemeteryCard(PlayerId playerId, Card card)
    {
        if (this.YouId == playerId)
        {
            this.youCemeteryCardListViewController.AddCard(card);
        }
        else
        {
            this.opponentCemeteryCardListViewController.AddCard(card);
        }
    }

    private void RemoveCemeteryCard(PlayerId playerId, Card card)
    {
        if (this.YouId == playerId)
        {
            this.youCemeteryCardListViewController.RemoveCard(card);
        }
        else
        {
            this.opponentCemeteryCardListViewController.RemoveCard(card);
        }
    }

    private void AddExcludedCard(Card card)
    {
        if (this.YouId == card.OwnerId)
        {
            this.youExcludedCardListViewController.AddCard(card);
        }
        else
        {
            this.opponentExcludedCardListViewController.AddCard(card);
        }
    }

    public void ShowEndGameDialog(EndGameNotifyMessage notify)
    {
        var title = "ゲーム終了";
        var isWin = notify.WinnerPlayerId == this.youPlayerController.PlayerId;
        var winOrLoseText = isWin
            ? "Win !"
            : "Lose...";

        var reasonText = notify.EndGameReason switch
        {
            EndGameReason.HpIsZero => $"{(isWin ? "相手" : "あなた")}のHPが0になりました。",
            EndGameReason.CardEffect => $"「{notify.EffectOwnerCard?.Name ?? ""}」の効果でゲームが終了しました。",
            EndGameReason.Surrender => $"{(isWin ? "相手" : "あなた")}が降参しました。",
            _ => throw new NotImplementedException($"正しくない値が指定されました。EndGameReason={notify.EndGameReason}"),
        };

        var message = winOrLoseText + Environment.NewLine + reasonText;

        this.PlayAudio(isWin ? SeAudioCache.SeAudioType.Win : SeAudioCache.SeAudioType.Lose);

        var dialog = Instantiate(this.confirmDialogPrefab);
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
        var title = "降参";
        var message = "降参しますか？";
        var dialog = Instantiate(this.confirmDialogPrefab);
        dialog.Init(title, message, ConfirmDialogController.DialogType.Confirm,
            onOkAction: async () =>
            {
                await this.Client.Surrender();
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    private async UniTask PlayAudio(string cardName, CardAudioCache.CardAudioType cardAudioType)
    {
        var (b, a) = await CardAudioCache.GetOrInit(cardName, cardAudioType);
        if (b)
        {
            this.audioSource.PlayOneShot(a);
        }
    }

    private void PlayAudio(SeAudioCache.SeAudioType audioType)
    {
        var (b, a) = SeAudioCache.GetOrInit(audioType);
        if (b)
        {
            this.audioSource.PlayOneShot(a);
        }
    }

    void OnStartTurn(GameContext gameContext, StartTurnNotifyMessage message)
    {
        Debug.Log($"OnStartTurn({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (this.youPlayerController.PlayerId == message.PlayerId)
            {
                await this.AddActionLog(new ActionLog("ターン開始", gameContext.You.PublicPlayerInfo));

                this.youPlayerController.SetActiveTurn(true);
                this.opponentPlayerController.SetActiveTurn(false);
                this.endTurnButton.interactable = true;
                await this.Client.StartTurn();
            }
            else
            {
                await this.AddActionLog(new ActionLog("ターン開始", gameContext.Opponent));

                this.youPlayerController.SetActiveTurn(false);
                this.opponentPlayerController.SetActiveTurn(true);
                this.endTurnButton.interactable = false;
            }
        });
    }

    void OnEndGame(GameContext gameContext, EndGameNotifyMessage message)
    {
        Debug.Log($"OnEndGame({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(() =>
        {
            // 戦闘結果のログを追加
            var localdata = LocalData.LoadLocalData();
            localdata.AddBattleLog(LocalBattleLog.Create(
                this.Client.GameId,
                this.currentGameContext.You.PublicPlayerInfo.Id,
                this.currentGameContext.You.PublicPlayerInfo.Name,
                this.currentGameContext.Opponent.Name,
                message.WinnerPlayerId == this.youPlayerController.PlayerId
            ));

            LocalData.SaveLocalData();

            this.ShowEndGameDialog(message);

            return UniTask.CompletedTask;
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

            Debug.Log($"プレイ: {carddef.Name}({ownerName})");

            //TODO carddefじゃなくてcardは取れない？
            //await this.AddActionLog(new ActionLog($"プレイ: {carddef.Name}({ownerName})", card));

            if (carddef.Type == CardType.Sorcery)
            {
                await this.PlayAudio(carddef?.Name ?? "", CardAudioCache.CardAudioType.Play);
            }
            await this.DisplaySmallCardDetail(carddef);

            //await this.UpdateGameContext(gameContext);
        });
    }

    void OnAddCard(GameContext gameContext, AddCardNotifyMessage message)
    {
        Debug.Log($"OnAddCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            this.currentGameContext = gameContext;

            var (ownerName, card) = Utility.GetCard(gameContext,
                message.ToZone,
                message.CardId);
            var playerName = Utility.GetPlayerName(gameContext, message.ToZone.PlayerId);

            await this.AddActionLog(new ActionLog("追加", card), message.EffectOwnerCard, message.EffectId);

            switch (message.ToZone.ZoneName)
            {
                case ZoneName.Hand:
                    await this.PlayAudio(card?.Name ?? "", CardAudioCache.CardAudioType.Draw);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.2));

                    if (message.ToZone.PlayerId == this.YouId)
                    {
                        this.GetOrCreateHandCardObject(card.Id, card, message.Index);
                    }
                    break;

                case ZoneName.Field:
                    await this.PlayAudio(card?.Name ?? "", CardAudioCache.CardAudioType.AddField);

                    var fieldCardObj = this.GetOrCreateFieldCardObject(card, message.ToZone.PlayerId, message.Index);
                    await fieldCardObj.transform.DOScale(1.4f, 0);
                    await fieldCardObj.transform.DOScale(1f, 0.3f);
                    break;

                case ZoneName.Cemetery:
                    this.AddCemeteryCard(message.ToZone.PlayerId, card);
                    break;

                default:
                    break;
            }

            await this.UpdateGameContextSimple(gameContext);
        });
    }

    void OnExcludeCard(GameContext gameContext, ExcludeCardNotifyMessage message)
    {
        Debug.Log($"OnExcludeCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var ownerName = Utility.GetPlayerName(gameContext, message.Card.OwnerId);

            await this.AddActionLog(
                new ActionLog($"{Utility.DisplayText(message.FromZone.ZoneName)}→除外", message.Card)
                , message.EffectOwnerCard, message.EffectId);

            await this.PlayAudio(message.Card?.Name ?? "", CardAudioCache.CardAudioType.Exclude);

            if (this.fieldCardControllersByCardId.TryGetValue(message.Card.Id, out var fieldCardController)
                && fieldCardController.Card.Type != CardType.Sorcery)
            {
                await fieldCardController.ExcludeEffect();
            }
            else if (this.handCardObjectsByCardId.TryGetValue(message.Card.Id, out var handCardController))
            {
                await handCardController.ExcludeEffect();
            }

            this.AddExcludedCard(message.Card);

            if (message.FromZone.ZoneName == ZoneName.Cemetery)
            {
                this.RemoveCemeteryCard(message.FromZone.PlayerId, message.Card);
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnMoveCard(GameContext gameContext, MoveCardNotifyMessage message)
    {
        Debug.Log($"OnMoveCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            this.currentGameContext = gameContext;

            var ownerName = Utility.GetPlayerName(gameContext, message.Card.OwnerId);
            var playerName = Utility.GetPlayerName(gameContext, message.ToZone.PlayerId);

            await this.AddActionLog(
                new ActionLog($"移動 {Utility.DisplayText(message.FromZone.ZoneName)}→{Utility.DisplayText(message.ToZone.ZoneName)}", message.Card),
                message.EffectOwnerCard, message.EffectId
                );

            var cardId = message.Card.Id;
            var targetPlayer = message.ToZone.PlayerId == this.YouId
                ? this.youPlayerController
                : this.opponentPlayerController;

            switch (message.FromZone.ZoneName)
            {
                case ZoneName.Cemetery:
                    {
                        this.RemoveCemeteryCard(message.FromZone.PlayerId, message.Card);
                        break;
                    }

                default:
                    break;
            }

            switch (message.ToZone.ZoneName)
            {
                case ZoneName.Cemetery:
                    {
                        if (message.FromZone.ZoneName == ZoneName.Field)
                        {
                            await this.PlayAudio(message.Card?.Name ?? "", CardAudioCache.CardAudioType.Destroy);
                        }

                        if (this.fieldCardControllersByCardId.TryGetValue(cardId, out var fieldCardController)
                            && fieldCardController.Card.Type != CardType.Sorcery)
                        {
                            await fieldCardController.DestroyEffect();
                        }
                        else if (this.handCardObjectsByCardId.TryGetValue(cardId, out var handCardController))
                        {
                            await handCardController.DestroyEffect();
                        }

                        this.AddCemeteryCard(message.ToZone.PlayerId, message.Card);
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
                        await this.PlayAudio(message.Card?.Name ?? "", CardAudioCache.CardAudioType.Draw);
                        await UniTask.Delay(TimeSpan.FromSeconds(0.2));

                        if (this.fieldCardControllersByCardId.TryGetValue(cardId, out var fieldCardController)
                            && fieldCardController.Card.Type != CardType.Sorcery)
                        {
                            await fieldCardController.BounceHandEffect(targetPlayer);
                        }

                        if (message.ToZone.PlayerId == this.YouId)
                        {
                            this.GetOrCreateHandCardObject(message.Card.Id, message.Card, message.Index);
                        }

                        break;
                    }

                case ZoneName.Field:
                    {
                        await this.PlayAudio(message.Card?.Name ?? "", CardAudioCache.CardAudioType.AddField);

                        var fieldCard = this.GetOrCreateFieldCardObject(message.Card, message.ToZone.PlayerId, message.Index);
                        await fieldCard.transform.DOScale(1.4f, 0);
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

    void OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage message)
    {
        Debug.Log($"OnModifyCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var ownerName = Utility.GetPlayerName(gameContext, message.Card.OwnerId);

            await this.AddActionLog(new ActionLog("修整", message.Card), message.EffectOwnerCard, message.EffectId);

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage message)
    {
        Debug.Log($"OnModifyPlayer({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var playerInfo = message.PlayerId == this.YouId
                    ? gameContext.You.PublicPlayerInfo
                    : gameContext.Opponent;

            await this.AddActionLog(
                new ActionLog("修整", playerInfo),
                message.EffectOwnerCard, message.EffectId
                );

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

    private async UniTask HealOrDamageEffect(PlayerController playerController, int oldHp, int newHp)
    {
        if (playerController == null) return;

        var diffHp = newHp - oldHp;

        if (diffHp != 0)
        {
            if (diffHp > 0)
            {
                await this.HealEffect(playerController, diffHp);
            }
            else
            {
                await playerController.DamageEffect(diffHp);
            }
        }
    }

    private async UniTask HealEffect(PlayerController playerController, int diffHp)
    {
        if (playerController == null) return;

        if (diffHp <= 0)
        {
            return;
        }

        await this.PlayAudio("", CardAudioCache.CardAudioType.Heal);
        await playerController.HealEffect(diffHp);
    }

    void OnModifyCounter(GameContext gameContext, ModifyCounterNotifyMessage message)
    {
        Debug.Log($"OnModifyCounter({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (message.TargetCard != default)
            {
                var ownerName = Utility.GetPlayerName(gameContext, message.TargetCard.OwnerId);

                await this.AddActionLog(
                    new ActionLog($"カウンター {message.CounterName}({message.NumCounters})", message.TargetCard),
                    message.EffectOwnerCard, message.EffectId
                    );
            }
            else if (message.TargetPlayerId != default)
            {
                var playerName = Utility.GetPlayerName(gameContext, message.TargetPlayerId);
                var playerInfo = message.TargetPlayerId == this.YouId
                    ? gameContext.You.PublicPlayerInfo
                    : gameContext.Opponent;

                await this.AddActionLog(
                    new ActionLog($"カウンター {message.CounterName}({message.NumCounters})", playerInfo),
                    message.EffectOwnerCard, message.EffectId
                    );
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnBattleStart(GameContext gameContext, BattleNotifyMessage message)
    {
        Debug.Log($"OnBattle({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            var (ownerPlayerName, attackCard) = Utility.GetCardAndOwner(gameContext, message.AttackCardId);
            if (message.GuardCardId == default)
            {
                var guardPlayerName = Utility.GetPlayerName(gameContext, message.GuardPlayerId);

                await this.AddActionLog(new ActionLog($"戦闘 {guardPlayerName}", attackCard));
            }
            else
            {
                var (guardCardOwnerName, guardCard) = Utility.GetCardAndOwner(gameContext, message.GuardCardId);

                await this.AddActionLog(new ActionLog($"戦闘 {guardCard.Name}", attackCard));
            }

            if (fieldCardControllersByCardId.TryGetValue(message.AttackCardId, out var attackCardController))
            {
                await this.PlayAudio(attackCardController.Card?.Name ?? "", CardAudioCache.CardAudioType.Attack);

                if (message.GuardCardId == default)
                {
                    if (this.youPlayerController.PlayerId == message.GuardPlayerId)
                    {
                        await attackCardController.AttackEffect(this.youPlayerController);
                    }
                    else
                    {
                        await attackCardController.AttackEffect(this.opponentPlayerController);
                    }
                }
                else
                {
                    if (fieldCardControllersByCardId.TryGetValue(message.GuardCardId, out var guardCard))
                    {
                        await attackCardController.AttackEffect(guardCard);
                    }
                }
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnBattleEnd(GameContext gameContext, BattleNotifyMessage message)
    {
        Debug.Log($"OnBattleEnd({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
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
                        var guardPlayerInfo = message.GuardPlayerId == this.YouId
                            ? gameContext.You.PublicPlayerInfo
                            : gameContext.Opponent;

                        await this.AddActionLog(
                            new ActionLog($"ダメージ {message.Damage}", guardPlayerInfo),
                            message.EffectOwnerCard, message.EffectId
                            );

                        break;
                    }

                case DamageNotifyMessage.ReasonValue.Attack:
                case DamageNotifyMessage.ReasonValue.Effect:
                    {
                        if (message.GuardCardId == default)
                        {
                            var guardPlayerInfo = message.GuardPlayerId == this.YouId
                                ? gameContext.You.PublicPlayerInfo
                                : gameContext.Opponent;

                            await this.AddActionLog(
                                new ActionLog($"ダメージ {message.Damage}", guardPlayerInfo),
                                message.EffectOwnerCard, message.EffectId
                                );
                        }
                        else
                        {
                            var (guardCardOwnerName, guardCard) = Utility.GetCardAndOwner(gameContext, message.GuardCardId);

                            await this.AddActionLog(
                                new ActionLog($"ダメージ {message.Damage}", guardCard),
                                message.EffectOwnerCard, message.EffectId
                                );
                        }

                        break;
                    }
            }

            if (message.GuardCardId == default)
            {
                await this.PlayAudio("", CardAudioCache.CardAudioType.Damage);

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
                    await this.PlayAudio(fieldCard.Card?.Name ?? "", CardAudioCache.CardAudioType.Damage);
                    await fieldCard.DamageEffect(message.Damage);
                }
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnHeal(GameContext gameContext, HealNotifyMessage message)
    {
        Debug.Log($"OnHeal({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (message.TakePlayerId != default)
            {
                var playerInfo = message.TakePlayerId == this.YouId
                        ? gameContext.You.PublicPlayerInfo
                        : gameContext.Opponent;

                await this.AddActionLog(
                    new ActionLog("回復", playerInfo),
                    message.EffectOwnerCard, message.EffectId
                    );

                if (this.youPlayerController.PlayerId == message.TakePlayerId)
                {
                    await this.HealEffect(youPlayerController, message.HealValue);
                }
                else if (this.opponentPlayerController.PlayerId == message.TakePlayerId)
                {
                    await this.HealEffect(opponentPlayerController, message.HealValue);
                }
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    void OnAsk(AskMessage message)
    {
        Debug.Log($"questionId={message.QuestionId}");

        this.updateViewActionQueue.Enqueue(() =>
        {
            this.StartChoiceMode(message);

            // ダイアログで選択させるか、フィールドから選択させるかの判定
            var choiceFromDialog = message.ChoiceCandidates.CardDefList.Length != 0
                 || message.ChoiceCandidates.CardList
                     .Any(x => x.Zone.ZoneName == ZoneName.Cemetery
                         || x.Zone.ZoneName == ZoneName.Deck
                         || x.Zone.ZoneName == ZoneName.CardPool);

            if (choiceFromDialog)
            {
                this.ShowChoiceDialog();
                return UniTask.CompletedTask;
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

                if (this.handCardObjectsByCardId.TryGetValue(card.Id, out var handCardController))
                {
                    handCardController.VisiblePickCandidateIcon(true);
                }
            }

            return UniTask.CompletedTask;
        });
    }

    private void DisplaySmallCardDetailSimple(Card card)
    {
        this.cardDetailController.SetCard(card);
    }

    private async UniTask DisplaySmallCardDetail(CardDef cardDef)
    {
        this.cardDetailController.SetCardDef(cardDef);
        await cardDetailController.transform.DOScale(1.1f, 0);
        await cardDetailController.transform.DOScale(1f, 0.5f);
    }

    private void DisplayBigCardDetail(CardBridge card)
    {
        this.cardDetailViewController.Open(card);
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

        this.choiceService.Reset();

        if (this.playTargetHand != null)
        {
            this.playTargetHand.transform.SetParent(this.handCardsContainer.transform, false);
            this.playTargetHand.DisablePlayTarget();
            this.playTargetHand = null;
        }
    }
}
