using Assets.Scripts;
using Assets.Scripts.BattleScene;
using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class ReplaySceneController : MonoBehaviour
{
    public Color YouColor => this.battleSceneController.YouColor;
    public Color OpponentColor => this.battleSceneController.OpponentColor;

    public PlayerId YouId => this.battleSceneController.youPlayerController.PlayerId;

    [SerializeField]
    private BattleSceneController battleSceneController = default;

    private readonly Dictionary<CardId, HandCardController> handCardObjectsByCardId = new Dictionary<CardId, HandCardController>();
    private readonly Dictionary<CardId, FieldCardController> fieldCardControllersByCardId = new Dictionary<CardId, FieldCardController>();

    private readonly ConcurrentQueue<Func<UniTask>> updateViewActionQueue = new ConcurrentQueue<Func<UniTask>>();

    private ConnectionHolder connectionHolder;

    private Client Client => this.connectionHolder.Client;

    private bool updating;

    private readonly List<IDisposable> disposableList = new List<IDisposable>();

    private GameContext currentGameContext;

    private bool IsPlayable(CardId cardId) => this.currentGameContext?.You?.PlayableCards?.Contains(cardId) ?? false;

    private int MaxNumFields => this.battleSceneController.youFieldSpaces.Length;
    private int MaxNumHands => this.battleSceneController.youHandSpaces.Length;

    private bool isStartedGame = false;

    private GameReplay gameReplay;
    private PlayerId replayPlayerId;
    private int currentActionLogId;
    private bool isRequesting;
    private bool isEnd;
    private float prevTime;
    private Dictionary<CardDefId, CardDef> replayCardpool;
    private bool isPaused;
    private readonly float requestInterval = 0.1f;
    private bool initialized = false;

    private void OnDestroy()
    {
        foreach (var disposable in this.disposableList)
        {
            disposable.Dispose();
        }
    }

    public async UniTask Init(GameReplay gameReplay, PlayerId playerId, CardDef[] cardpool)
    {
        this.battleSceneController.battleUiContainer.SetActive(false);
        this.battleSceneController.replayUiContainer.SetActive(true);

        var holder = ConnectionHolder.Find();

        this.battleSceneController.youCemeteryCardListViewController.InitAsYou("Cemetery");
        this.battleSceneController.opponentCemeteryCardListViewController.InitAsOpponent("Cemetery");

        this.battleSceneController.youExcludedCardListViewController.InitAsYou("Excluded");
        this.battleSceneController.opponentExcludedCardListViewController.InitAsOpponent("Excluded");

        this.battleSceneController.cardDetailController.Init(this.DisplayBigCardDetail);

        this.battleSceneController.youPlayerController.Init(this.UnPick, this.Pick);
        this.battleSceneController.opponentPlayerController.Init(this.UnPick, this.Pick);

        this.disposableList.AddRange(new[]
        {
            holder.Receiver.OnPlayCard.Subscribe((a) => this.OnPlayCard(a.gameContext, a.message)),
            holder.Receiver.OnAddCard.Subscribe((a) => this.OnAddCard(a.gameContext, a.message)),
            //holder.Receiver.OnAsk.Subscribe((a) => this.OnAsk(a)),
            holder.Receiver.OnBattleStart.Subscribe((a) => this.OnBattleStart(a.gameContext, a.message)),
            holder.Receiver.OnBattleEnd.Subscribe((a) => this.OnBattleEnd(a.gameContext, a.message)),
            holder.Receiver.OnDamage.Subscribe((a) => this.OnDamage(a.gameContext, a.message)),
            holder.Receiver.OnHeal.Subscribe((a) => this.OnHeal(a.gameContext, a.message)),
            holder.Receiver.OnModifyCard.Subscribe((a) => this.OnModifyCard(a.gameContext, a.message)),
            holder.Receiver.OnModifyPlayer.Subscribe((a) => this.OnModifyPlayer(a.gameContext, a.message)),
            holder.Receiver.OnModifyCounter.Subscribe((a) => this.OnModifyCounter(a.gameContext, a.message)),
            holder.Receiver.OnModifyNumFields.Subscribe((a) => this.OnModifyNumFields(a.gameContext, a.message)),
            holder.Receiver.OnMoveCard.Subscribe((a) => this.OnMoveCard(a.gameContext, a.message)),
            holder.Receiver.OnExcludeCard.Subscribe((a) => this.OnExcludeCard(a.gameContext, a.message)),
            holder.Receiver.OnStartTurn.Subscribe((a) => this.OnStartTurn(a.gameContext, a.message)),
            holder.Receiver.OnEndGame.Subscribe((a) => this.OnEndGame(a.gameContext, a.message)),
        });

        this.connectionHolder = holder;

        // リプレイするプレイヤーのID
        this.battleSceneController.youPlayerController.Set(playerId);

        this.gameReplay = gameReplay;
        this.replayPlayerId = playerId;
        this.replayCardpool = cardpool.ToDictionary(x => x.Id, x => x);
        this.currentActionLogId = await this.Client.FirstActionLog(this.gameReplay.GameId);

        this.initialized = true;
    }

    private async void Update()
    {
        if (!this.initialized)
        {
            return;
        }

        if (this.gameReplay == null)
        {
            return;
        }

        if (this.isPaused)
        {
            return;
        }

        if (this.isEnd)
        {
            return;
        }

        if (Time.time - this.prevTime < this.requestInterval)
        {
            return;
        }

        this.prevTime = Time.time;

        if (this.updating)
        {
            return;
        }

        if (this.updateViewActionQueue.TryDequeue(out var updateViewAction))
        {
            this.battleSceneController.HideLoadingView();

            try
            {
                this.updating = true;
                await updateViewAction();
            }
            finally
            {
                this.updating = false;
            }
        }
        else
        {
            await this.RequestNextAction();
        }
    }

    public void Pick(PlayerController controller)
    {
    }

    public void UnPick(PlayerController controller)
    {
    }

    public void Pick(CardController controller)
    {
    }

    public void UnPick(CardController controller)
    {
    }

    private void SetPlayTargetHand(HandCardController target)
    {
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

    public void OnAutoReplayButtonClick()
    {
        this.StartAutoReplay();
    }

    public void OnPauseActionButtonClick()
    {
        this.PauseReplay();
    }

    public void OnEndReplayButtonClick()
    {
        this.PauseReplay();

        this.ShowConfirmEndReplayDialog();
    }

    private void StartAutoReplay()
    {
        this.isPaused = false;
    }

    private void PauseReplay()
    {
        this.isPaused = true;
    }

    private void ShowConfirmEndReplayDialog()
    {
        var title = "リプレイの終了";
        var message = "リプレイを終了しますか？";
        var dialog = Instantiate(this.battleSceneController.confirmDialogPrefab);
        dialog.Init(title, message, ConfirmDialogController.DialogType.Confirm,
            onOkAction: async () =>
            {
                await this.Client.LeaveRoom();
                await Utility.LoadAsyncScene(SceneNames.ListBattleLogsScene);
            });
        dialog.transform.SetParent(this.battleSceneController.canvas.transform, false);
    }

    private async UniTask RequestNextAction()
    {
        if (this.isRequesting)
        {
            return;
        }

        try
        {
            this.isRequesting = true;

            this.currentActionLogId = await this.Client.NextActionLog(
                this.gameReplay.GameId,
                this.replayPlayerId,
                this.currentActionLogId
                );

            if (this.currentActionLogId == -1)
            {
                this.isEnd = true;
            }
        }
        finally
        {
            this.isRequesting = false;
        }
    }

    private async UniTask AddActionLog(ActionLog actionLog, Card effectOwnerCard = default, CardEffectId? effectId = default)
    {
        await this.battleSceneController.actionLogViewController.AddLog(actionLog, effectOwnerCard, effectId);
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

            this.battleSceneController.youPlayerController.Set(publicInfo);

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
                this.UpdateField(publicInfo, fieldIndex, youFieldCards[fieldIndex]);
            }
        }

        var opponent = gameContext.Opponent;
        if (opponent != null)
        {
            this.battleSceneController.opponentPlayerController.Set(opponent);

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
            foreach (var fieldIndex in Enumerable.Range(0, Mathf.Min(opponentFieldCards.Length, MaxNumFields)))
            {
                this.UpdateField(opponent, fieldIndex, opponentFieldCards[fieldIndex]);
            }
        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.3));
    }

    private void UpdateField(PublicPlayerInfo publicInfo, int fieldIndex, Card fieldCard)
    {
        var fieldSpace = publicInfo.Id == this.YouId
            ? this.battleSceneController.youFieldSpaces[fieldIndex]
            : this.battleSceneController.opponentFieldSpaces[fieldIndex];

        if (publicInfo.IsAvailableFields[fieldIndex])
        {
            // 場が有効なら
            fieldSpace.SetEnable(true);

            if (fieldCard != null)
            {
                this.GetOrCreateFieldCardObject(fieldCard, publicInfo.Id, fieldIndex);
            }
        }
        else
        {
            // 場が無効なら
            fieldSpace.SetEnable(false);
        }
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
            this.battleSceneController.youPlayerController.Set(publicInfo);
        }

        var opponent = gameContext.Opponent;
        if (opponent != null)
        {
            this.battleSceneController.opponentPlayerController.Set(opponent);

        }

        await UniTask.Delay(TimeSpan.FromSeconds(0.3));
    }

    private HandCardController GetOrCreateHandCardObject(CardId cardId, Card card, int index)
    {
        if (!handCardObjectsByCardId.TryGetValue(cardId, out var controller))
        {
            this.RemoveCardObjectByCardId(cardId);

            controller = Instantiate(this.battleSceneController.handCardPrefab, this.battleSceneController.handCardsContainer.transform);
            handCardObjectsByCardId.Add(cardId, controller);
        }

        controller.Init(card, this.DisplaySmallCardDetailSimple,
            this.UnPick,
            this.Pick,
            this.SetPlayTargetHand
            );

        controller.transform.position = this.battleSceneController.youHandSpaces[index].transform.position;

        controller.SetCanPlay(this.IsPlayable(cardId));

        return controller;
    }

    private FieldCardController GetOrCreateFieldCardObject(Card card, PlayerId playerId, int index)
    {
        if (!fieldCardControllersByCardId.TryGetValue(card.Id, out var cardController))
        {
            this.RemoveCardObjectByCardId(card.Id);

            cardController = Instantiate(this.battleSceneController.fieldCardPrefab, this.battleSceneController.fieldCardsContainer.transform);
            fieldCardControllersByCardId.Add(card.Id, cardController);
        }

        cardController.Init(card, this.DisplaySmallCardDetailSimple,
            this.UnPick,
            this.Pick
            );

        cardController.transform.position = playerId == this.YouId
            ? this.battleSceneController.youFieldSpaces[index].transform.position
            : this.battleSceneController.opponentFieldSpaces[index].transform.position;

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
            this.battleSceneController.youCemeteryCardListViewController.AddCard(card);
        }
        else
        {
            this.battleSceneController.opponentCemeteryCardListViewController.AddCard(card);
        }
    }

    private void RemoveCemeteryCard(PlayerId playerId, Card card)
    {
        if (this.YouId == playerId)
        {
            this.battleSceneController.youCemeteryCardListViewController.RemoveCard(card);
        }
        else
        {
            this.battleSceneController.opponentCemeteryCardListViewController.RemoveCard(card);
        }
    }

    private void AddExcludedCard(Card card)
    {
        if (this.YouId == card.OwnerId)
        {
            this.battleSceneController.youExcludedCardListViewController.AddCard(card);
        }
        else
        {
            this.battleSceneController.opponentExcludedCardListViewController.AddCard(card);
        }
    }

    void OnStartTurn(GameContext gameContext, StartTurnNotifyMessage message)
    {
        Debug.Log($"OnStartTurn({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (this.battleSceneController.youPlayerController.PlayerId == message.PlayerId)
            {
                await this.AddActionLog(new ActionLog("ターン開始", gameContext.You.PublicPlayerInfo));

                await this.battleSceneController.startTurnMessage.Show(true, !this.isStartedGame,
                    gameContext.Opponent.TurnCount,
                    gameContext.You.PublicPlayerInfo.TurnCount + 1
                    );

                this.battleSceneController.youPlayerController.SetActiveTurn(true);
                this.battleSceneController.opponentPlayerController.SetActiveTurn(false);
            }
            else
            {
                await this.AddActionLog(new ActionLog("ターン開始", gameContext.Opponent));

                await this.battleSceneController.startTurnMessage.Show(false, !this.isStartedGame,
                    gameContext.You.PublicPlayerInfo.TurnCount,
                    gameContext.Opponent.TurnCount + 1
                    );

                this.battleSceneController.youPlayerController.SetActiveTurn(false);
                this.battleSceneController.opponentPlayerController.SetActiveTurn(true);
            }

            if (!this.isStartedGame)
            {
                this.isStartedGame = true;
            }
        });
    }

    void OnEndGame(GameContext gameContext, EndGameNotifyMessage message)
    {
        Debug.Log($"OnEndGame({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(() =>
        {
            // 戦闘結果のログを追加
            var dialog = Instantiate(this.battleSceneController.confirmDialogPrefab);
            EndGameDialog.ShowEndGameDialog(
                message,
                dialog,
                this.battleSceneController.canvas,
                this.battleSceneController.youPlayerController.PlayerId,
                onOkAction: async () =>
                {
                    await this.Client.LeaveRoom();
                    await Utility.LoadAsyncScene(SceneNames.ListBattleLogsScene);
                });

            return UniTask.CompletedTask;
        });
    }

    void OnPlayCard(GameContext gameContext, PlayCardNotifyMessage message)
    {
        Debug.Log($"OnPlayCard({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (!this.replayCardpool.TryGetValue(message.CardDefId, out var carddef))
            {
                Debug.Log("Undefined Play Card id=" + message.CardDefId);
                return;
            }

            var ownerName = Utility.GetPlayerName(gameContext, message.PlayerId);

            Debug.Log($"プレイ: {carddef.Name}({ownerName})");

            if (carddef.Type == CardType.Sorcery)
            {
                await AudioController.CreateOrFind().PlaySe(carddef?.Name ?? "", CardAudioCache.CardAudioType.Play);
            }
            await this.DisplaySmallCardDetail(carddef);
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
                    await AudioController.CreateOrFind().PlaySe(card?.Name ?? "", CardAudioCache.CardAudioType.Draw);
                    await UniTask.Delay(TimeSpan.FromSeconds(0.2));

                    if (message.ToZone.PlayerId == this.YouId)
                    {
                        this.GetOrCreateHandCardObject(card.Id, card, message.Index);
                    }
                    break;

                case ZoneName.Field:
                    await AudioController.CreateOrFind().PlaySe(card?.Name ?? "", CardAudioCache.CardAudioType.AddField);

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

            await AudioController.CreateOrFind().PlaySe(message.Card?.Name ?? "", CardAudioCache.CardAudioType.Exclude);

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
                ? this.battleSceneController.youPlayerController
                : this.battleSceneController.opponentPlayerController;

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
                            await AudioController.CreateOrFind().PlaySe(message.Card?.Name ?? "", CardAudioCache.CardAudioType.Destroy);
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
                        await AudioController.CreateOrFind().PlaySe(message.Card?.Name ?? "", CardAudioCache.CardAudioType.Draw);
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
                        await AudioController.CreateOrFind().PlaySe(message.Card?.Name ?? "", CardAudioCache.CardAudioType.AddField);

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

            if (this.battleSceneController.youPlayerController.PlayerId == message.PlayerId)
            {
                await HealOrDamageEffect(this.battleSceneController.youPlayerController,
                    this.currentGameContext.You.PublicPlayerInfo.CurrentHp,
                    gameContext.You.PublicPlayerInfo.CurrentHp);
            }
            else if (this.battleSceneController.opponentPlayerController.PlayerId == message.PlayerId)
            {
                await HealOrDamageEffect(this.battleSceneController.opponentPlayerController,
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

        await AudioController.CreateOrFind().PlaySe("", CardAudioCache.CardAudioType.Heal);
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

    void OnModifyNumFields(GameContext gameContext, ModifyNumFieldsNotifyMessage message)
    {
        Debug.Log($"OnModifyNumFields({this.Client.PlayerName})");

        this.updateViewActionQueue.Enqueue(async () =>
        {
            if (message.PlayerId != default)
            {
                var playerName = Utility.GetPlayerName(gameContext, message.PlayerId);
                var playerInfo = message.PlayerId == this.YouId
                    ? gameContext.You.PublicPlayerInfo
                    : gameContext.Opponent;

                await this.AddActionLog(
                    new ActionLog($"場の数が変化", playerInfo),
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
                await AudioController.CreateOrFind().PlaySe(attackCardController.Card?.Name ?? "", CardAudioCache.CardAudioType.Attack);

                if (message.GuardCardId == default)
                {
                    if (this.battleSceneController.youPlayerController.PlayerId == message.GuardPlayerId)
                    {
                        await attackCardController.AttackEffect(this.battleSceneController.youPlayerController);
                    }
                    else
                    {
                        await attackCardController.AttackEffect(this.battleSceneController.opponentPlayerController);
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
                await AudioController.CreateOrFind().PlaySe("", CardAudioCache.CardAudioType.Damage);

                if (this.battleSceneController.youPlayerController.PlayerId == message.GuardPlayerId)
                {
                    await this.battleSceneController.youPlayerController.DamageEffect(message.Damage);
                }
                else
                {
                    await this.battleSceneController.opponentPlayerController.DamageEffect(message.Damage);
                }
            }
            else
            {
                if (fieldCardControllersByCardId.TryGetValue(message.GuardCardId, out var fieldCard))
                {
                    await AudioController.CreateOrFind().PlaySe(fieldCard.Card?.Name ?? "", CardAudioCache.CardAudioType.Damage);
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

                if (this.battleSceneController.youPlayerController.PlayerId == message.TakePlayerId)
                {
                    await this.HealEffect(this.battleSceneController.youPlayerController, message.HealValue);
                }
                else if (this.battleSceneController.opponentPlayerController.PlayerId == message.TakePlayerId)
                {
                    await this.HealEffect(this.battleSceneController.opponentPlayerController, message.HealValue);
                }
            }

            await this.UpdateGameContext(gameContext);
        });
    }

    private void DisplaySmallCardDetailSimple(Card card)
    {
        this.battleSceneController.cardDetailController.SetCard(card);
    }

    private async UniTask DisplaySmallCardDetail(CardDef cardDef)
    {
        this.battleSceneController.cardDetailController.SetCardDef(cardDef);
        await this.battleSceneController.cardDetailController.transform.DOScale(1.1f, 0);
        await this.battleSceneController.cardDetailController.transform.DOScale(1f, 0.5f);
    }

    private void DisplayBigCardDetail(CardBridge card)
    {
        this.battleSceneController.cardDetailViewController.Open(card);
    }
}
