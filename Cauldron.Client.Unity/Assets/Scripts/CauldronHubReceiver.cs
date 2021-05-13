using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using System;
using UniRx;

namespace Assets.Scripts
{
    public class CauldronHubReceiver : ICauldronHubReceiver
    {
        public IObservable<(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage)> OnAddCard => onAddCard;
        private readonly Subject<(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage)> onAddCard
            = new Subject<(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage)>();
        void ICauldronHubReceiver.OnAddCard(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage)
            => this.onAddCard.OnNext((gameContext, addCardNotifyMessage));

        public IObservable<AskMessage> OnAsk => onAsk;
        private readonly Subject<AskMessage> onAsk = new Subject<AskMessage>();
        void ICauldronHubReceiver.OnAsk(AskMessage onAskMessage)
            => this.onAsk.OnNext(onAskMessage);

        public IObservable<(GameContext gameContext, DamageNotifyMessage damageNotifyMessage)> OnDamage => onDamage;
        private readonly Subject<(GameContext gameContext, DamageNotifyMessage damageNotifyMessage)> onDamage
            = new Subject<(GameContext gameContext, DamageNotifyMessage damageNotifyMessage)>();
        void ICauldronHubReceiver.OnDamage(GameContext gameContext, DamageNotifyMessage damageNotifyMessage)
            => this.onDamage.OnNext((gameContext, damageNotifyMessage));

        public IObservable<GameContext> OnGameOver => onGameOver;
        private readonly Subject<GameContext> onGameOver = new Subject<GameContext>();
        void ICauldronHubReceiver.OnGameOver(GameContext gameContext)
            => this.onGameOver.OnNext(gameContext);

        public IObservable<(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage)> OnModifyCard => onModifyCard;
        private readonly Subject<(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage)> onModifyCard
            = new Subject<(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage)>();
        void ICauldronHubReceiver.OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage)
            => this.onModifyCard.OnNext((gameContext, modifyCardNotifyMessage));

        public IObservable<(GameContext gameContext, ModifyPlayerNotifyMessage modifyPlayerNotifyMessage)> OnModifyPlayer => onModifyPlayer;
        private readonly Subject<(GameContext gameContext, ModifyPlayerNotifyMessage modifyPlayerNotifyMessage)> onModifyPlayer
            = new Subject<(GameContext gameContext, ModifyPlayerNotifyMessage modifyPlayerNotifyMessage)>();
        void ICauldronHubReceiver.OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage modifyPlayerNotifyMessage)
            => this.onModifyPlayer.OnNext((gameContext, modifyPlayerNotifyMessage));

        public IObservable<(GameContext gameContext, MoveCardNotifyMessage moveCardNotifyMessage)> OnMoveCard => onMoveCard;
        private readonly Subject<(GameContext gameContext, MoveCardNotifyMessage moveCardNotifyMessage)> onMoveCard
            = new Subject<(GameContext gameContext, MoveCardNotifyMessage moveCardNotifyMessage)>();
        void ICauldronHubReceiver.OnMoveCard(GameContext gameContext, MoveCardNotifyMessage moveCardNotifyMessage)
            => this.onMoveCard.OnNext((gameContext, moveCardNotifyMessage));

        public IObservable<GameContext> OnReady => onReady;
        private readonly Subject<GameContext> onReady = new Subject<GameContext>();
        void ICauldronHubReceiver.OnReady(GameContext gameContext)
            => this.onReady.OnNext(gameContext);

        public IObservable<GameContext> OnStartGame => onStartGame;
        private readonly Subject<GameContext> onStartGame = new Subject<GameContext>();
        void ICauldronHubReceiver.OnStartGame(GameContext gameContext)
            => this.onStartGame.OnNext(gameContext);

        public IObservable<(GameContext gameContext, PlayerId playerId)> OnStartTurn => onStartTurn;
        private readonly Subject<(GameContext gameContext, PlayerId playerId)> onStartTurn
            = new Subject<(GameContext gameContext, PlayerId playerId)>();
        void ICauldronHubReceiver.OnStartTurn(GameContext gameContext, PlayerId playerId)
            => this.onStartTurn.OnNext((gameContext, playerId));

        public IObservable<Unit> OnJoinGame => onJoinGame;
        private readonly Subject<Unit> onJoinGame = new Subject<Unit>();
        void ICauldronHubReceiver.OnJoinGame()
            => this.onJoinGame.OnNext(Unit.Default);
    }
}
