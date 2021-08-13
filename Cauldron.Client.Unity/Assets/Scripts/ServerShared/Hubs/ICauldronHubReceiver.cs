using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Shared.Services
{
    public interface ICauldronHubReceiver
    {
        void OnJoinGame();
        void OnReady();
        void OnStartGame();
        void OnEndGame(GameContext gameContext);
        void OnStartTurn(GameContext gameContext, PlayerId playerId);
        void OnAddCard(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage);
        void OnExcludeCard(GameContext gameContext, ExcludeCardNotifyMessage message);
        void OnMoveCard(GameContext gameContext, MoveCardNotifyMessage moveCardNotifyMessage);
        void OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage);
        void OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage modifyPlayerNotifyMessage);
        void OnDamage(GameContext gameContext, DamageNotifyMessage damageNotifyMessage);
        void OnBattle(GameContext gameContext, BattleNotifyMessage message);

        void OnAsk(AskMessage choiceCardsMessage);
    }
}