using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using System;
using UniRx;

namespace Assets.Scripts
{
    public class CauldronHubReceiver : ICauldronHubReceiver
    {
        public IObservable<(GameContext gameContext, PlayCardNotifyMessage message)> OnPlayCard => onPlayCard;
        private readonly Subject<(GameContext gameContext, PlayCardNotifyMessage message)> onPlayCard
            = new Subject<(GameContext gameContext, PlayCardNotifyMessage message)>();
        void ICauldronHubReceiver.OnPlayCard(GameContext gameContext, PlayCardNotifyMessage message)
            => this.onPlayCard.OnNext((gameContext, message));

        public IObservable<(GameContext gameContext, AddCardNotifyMessage message)> OnAddCard => onAddCard;
        private readonly Subject<(GameContext gameContext, AddCardNotifyMessage message)> onAddCard
            = new Subject<(GameContext gameContext, AddCardNotifyMessage message)>();
        void ICauldronHubReceiver.OnAddCard(GameContext gameContext, AddCardNotifyMessage message)
            => this.onAddCard.OnNext((gameContext, message));

        public IObservable<(GameContext gameContext, ExcludeCardNotifyMessage message)> OnExcludeCard => onExcludeCard;
        private readonly Subject<(GameContext gameContext, ExcludeCardNotifyMessage message)> onExcludeCard
            = new Subject<(GameContext gameContext, ExcludeCardNotifyMessage message)>();
        void ICauldronHubReceiver.OnExcludeCard(GameContext gameContext, ExcludeCardNotifyMessage message)
            => this.onExcludeCard.OnNext((gameContext, message));

        public IObservable<AskMessage> OnAsk => onAsk;
        private readonly Subject<AskMessage> onAsk = new Subject<AskMessage>();
        void ICauldronHubReceiver.OnAsk(AskMessage onAskMessage)
            => this.onAsk.OnNext(onAskMessage);

        public IObservable<(GameContext gameContext, DamageNotifyMessage message)> OnDamage => onDamage;
        private readonly Subject<(GameContext gameContext, DamageNotifyMessage message)> onDamage
            = new Subject<(GameContext gameContext, DamageNotifyMessage message)>();
        void ICauldronHubReceiver.OnDamage(GameContext gameContext, DamageNotifyMessage message)
            => this.onDamage.OnNext((gameContext, message));

        public IObservable<(GameContext gameContext, BattleNotifyMessage message)> OnBattleStart => onBattleStart;
        private readonly Subject<(GameContext gameContext, BattleNotifyMessage message)> onBattleStart
            = new Subject<(GameContext gameContext, BattleNotifyMessage message)>();
        void ICauldronHubReceiver.OnBattleStart(GameContext gameContext, BattleNotifyMessage message)
            => this.onBattleStart.OnNext((gameContext, message));

        public IObservable<(GameContext gameContext, BattleNotifyMessage message)> OnBattleEnd => onBattleEnd;
        private readonly Subject<(GameContext gameContext, BattleNotifyMessage message)> onBattleEnd
            = new Subject<(GameContext gameContext, BattleNotifyMessage message)>();
        void ICauldronHubReceiver.OnBattleEnd(GameContext gameContext, BattleNotifyMessage message)
            => this.onBattleEnd.OnNext((gameContext, message));

        public IObservable<(GameContext gameContext, ModifyCardNotifyMessage message)> OnModifyCard => onModifyCard;
        private readonly Subject<(GameContext gameContext, ModifyCardNotifyMessage message)> onModifyCard
            = new Subject<(GameContext gameContext, ModifyCardNotifyMessage message)>();
        void ICauldronHubReceiver.OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage message)
            => this.onModifyCard.OnNext((gameContext, message));

        public IObservable<(GameContext gameContext, ModifyCounterNotifyMessage message)> OnModifyCounter => onModifyCounter;
        private readonly Subject<(GameContext gameContext, ModifyCounterNotifyMessage message)> onModifyCounter
            = new Subject<(GameContext gameContext, ModifyCounterNotifyMessage message)>();
        void ICauldronHubReceiver.OnModifyCounter(GameContext gameContext, ModifyCounterNotifyMessage message)
            => this.onModifyCounter.OnNext((gameContext, message));

        public IObservable<(GameContext gameContext, ModifyPlayerNotifyMessage message)> OnModifyPlayer => onModifyPlayer;
        private readonly Subject<(GameContext gameContext, ModifyPlayerNotifyMessage message)> onModifyPlayer
            = new Subject<(GameContext gameContext, ModifyPlayerNotifyMessage message)>();
        void ICauldronHubReceiver.OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage message)
            => this.onModifyPlayer.OnNext((gameContext, message));

        public IObservable<(GameContext gameContext, MoveCardNotifyMessage message)> OnMoveCard => onMoveCard;
        private readonly Subject<(GameContext gameContext, MoveCardNotifyMessage message)> onMoveCard
            = new Subject<(GameContext gameContext, MoveCardNotifyMessage message)>();
        void ICauldronHubReceiver.OnMoveCard(GameContext gameContext, MoveCardNotifyMessage message)
            => this.onMoveCard.OnNext((gameContext, message));

        public IObservable<Unit> OnReady => onReady;
        private readonly Subject<Unit> onReady = new Subject<Unit>();
        void ICauldronHubReceiver.OnReady()
            => this.onReady.OnNext(Unit.Default);

        public IObservable<Unit> OnStartGame => onStartGame;
        private readonly Subject<Unit> onStartGame = new Subject<Unit>();
        void ICauldronHubReceiver.OnStartGame()
            => this.onStartGame.OnNext(Unit.Default);

        public IObservable<(GameContext gameContext, StartTurnNotifyMessage message)> OnStartTurn => onStartTurn;
        private readonly Subject<(GameContext gameContext, StartTurnNotifyMessage message)> onStartTurn
            = new Subject<(GameContext gameContext, StartTurnNotifyMessage message)>();
        void ICauldronHubReceiver.OnStartTurn(GameContext gameContext, StartTurnNotifyMessage message)
            => this.onStartTurn.OnNext((gameContext, message));

        public IObservable<Unit> OnJoinGame => onJoinGame;
        private readonly Subject<Unit> onJoinGame = new Subject<Unit>();
        void ICauldronHubReceiver.OnJoinGame()
            => this.onJoinGame.OnNext(Unit.Default);

        public IObservable<(GameContext gameContext, EndGameNotifyMessage message)> OnEndGame => onEndGame;
        private readonly Subject<(GameContext, EndGameNotifyMessage)> onEndGame
            = new Subject<(GameContext, EndGameNotifyMessage)>();
        void ICauldronHubReceiver.OnEndGame(GameContext gameContext, EndGameNotifyMessage message)
            => this.onEndGame.OnNext((gameContext, message));
    }
}
