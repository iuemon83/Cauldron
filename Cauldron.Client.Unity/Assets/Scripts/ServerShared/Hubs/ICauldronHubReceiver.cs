using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Shared.Services
{
    public interface ICauldronHubReceiver
    {
        void OnJoinGame();
        void OnReady();
        void OnStartGame();
        void OnEndGame(GameContext gameContext, EndGameNotifyMessage message);
        void OnStartTurn(GameContext gameContext, StartTurnNotifyMessage message);
        void OnPlayCard(GameContext gameContext, PlayCardNotifyMessage message);
        void OnAddCard(GameContext gameContext, AddCardNotifyMessage message);
        void OnExcludeCard(GameContext gameContext, ExcludeCardNotifyMessage message);
        void OnMoveCard(GameContext gameContext, MoveCardNotifyMessage message);
        void OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage message);
        void OnModifyCounter(GameContext gameContext, ModifyCounterNotifyMessage message);
        void OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage message);
        void OnDamage(GameContext gameContext, DamageNotifyMessage message);
        void OnBattleStart(GameContext gameContext, BattleNotifyMessage message);
        void OnBattleEnd(GameContext gameContext, BattleNotifyMessage message);

        void OnAsk(AskMessage choiceCardsMessage);
    }
}
