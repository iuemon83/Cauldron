using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using System;
using UniRx;

namespace Assets.Scripts
{
    public class CauldronHubReceiver : ICauldronHubReceiver
    {
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

        public IObservable<(GameContext gameContext, BattleNotifyMessage message)> OnBattle => onBattle;
        private readonly Subject<(GameContext gameContext, BattleNotifyMessage message)> onBattle
            = new Subject<(GameContext gameContext, BattleNotifyMessage message)>();
        void ICauldronHubReceiver.OnBattle(GameContext gameContext, BattleNotifyMessage message)
            => this.onBattle.OnNext((gameContext, message));

        public IObservable<(GameContext gameContext, ModifyCardNotifyMessage message)> OnModifyCard => onModifyCard;
        private readonly Subject<(GameContext gameContext, ModifyCardNotifyMessage message)> onModifyCard
            = new Subject<(GameContext gameContext, ModifyCardNotifyMessage message)>();
        void ICauldronHubReceiver.OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage message)
            => this.onModifyCard.OnNext((gameContext, message));

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

        public IObservable<(GameContext gameContext, PlayerId playerId)> OnStartTurn => onStartTurn;
        private readonly Subject<(GameContext gameContext, PlayerId playerId)> onStartTurn
            = new Subject<(GameContext gameContext, PlayerId playerId)>();
        void ICauldronHubReceiver.OnStartTurn(GameContext gameContext, PlayerId playerId)
            => this.onStartTurn.OnNext((gameContext, playerId));

        public IObservable<Unit> OnJoinGame => onJoinGame;
        private readonly Subject<Unit> onJoinGame = new Subject<Unit>();
        void ICauldronHubReceiver.OnJoinGame()
            => this.onJoinGame.OnNext(Unit.Default);

        public IObservable<GameContext> OnEndGame => onEndGame;
        private readonly Subject<GameContext> onEndGame = new Subject<GameContext>();
        void ICauldronHubReceiver.OnEndGame(GameContext gameContext)
            => this.onEndGame.OnNext(gameContext);
    }
}
