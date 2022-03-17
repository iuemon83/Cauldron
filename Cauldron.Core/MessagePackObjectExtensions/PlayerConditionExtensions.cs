using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class PlayerConditionExtensions
    {
        public static Player[] ListMatchedPlayers(this PlayerCondition playerCondition, Card effectOwnerCard, EffectEventArgs eventArgs, PlayerRepository playerRepository)
        {
            return playerRepository.AllPlayers
                .Where(p => playerCondition.IsMatch(effectOwnerCard, eventArgs, p))
                .ToArray();
        }

        public static bool IsMatch(this PlayerCondition playerCondition, Card effectOwnerCard, EffectEventArgs eventArgs, Player playerToMatch)
        {
            return
                playerCondition.Context switch
                {
                    PlayerCondition.ContextValue.EventSource => playerToMatch.Id == eventArgs.SourcePlayer?.Id,
                    PlayerCondition.ContextValue.You => playerToMatch.Id == effectOwnerCard.OwnerId,
                    PlayerCondition.ContextValue.Opponent => playerToMatch.Id != effectOwnerCard.OwnerId,
                    PlayerCondition.ContextValue.Active => playerToMatch.Id == eventArgs.GameMaster.ActivePlayer.Id,
                    PlayerCondition.ContextValue.NonActive => playerToMatch.Id != eventArgs.GameMaster.ActivePlayer.Id,
                    PlayerCondition.ContextValue.ActionTarget => playerToMatch.Id == eventArgs.ActionTargetPlayer?.Id,
                    PlayerCondition.ContextValue.ActionTargetAll => eventArgs.ActionTargetPlayers.Select(x => x.Id).Contains(playerToMatch.Id),
                    _ => true
                };
        }
    }
}
