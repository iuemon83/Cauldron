using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Shared.Services
{
    public interface ICauldronHubReceiver
    {
        void OnJoinGame();
        void OnReady(GameContext gameContext);
        void OnStartGame(GameContext gameContext);
        void OnStartTurn(GameContext gameContext, PlayerId playerId);
        void OnAddCard(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage);
        void OnMoveCard(GameContext gameContext, MoveCardNotifyMessage moveCardNotifyMessage);
        void OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage);
        void OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage modifyPlayerNotifyMessage);
        void OnDamage(GameContext gameContext, DamageNotifyMessage damageNotifyMessage);

        void OnAsk(AskMessage choiceCardsMessage);
    }
}