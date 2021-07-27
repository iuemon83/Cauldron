using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;
using System.Linq;

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
                    PlayerCondition.ContextValue.EventSource => playerToMatch.Id == eventArgs.SourcePlayer.Id,
                    _ => true
                }
                && playerCondition.Type switch
                {
                    PlayerCondition.TypeValue.You => playerToMatch.Id == effectOwnerCard.OwnerId,
                    PlayerCondition.TypeValue.Opponent => playerToMatch.Id != effectOwnerCard.OwnerId,
                    PlayerCondition.TypeValue.Active => playerToMatch.Id == eventArgs.GameMaster.ActivePlayer.Id,
                    PlayerCondition.TypeValue.NonActive => playerToMatch.Id != eventArgs.GameMaster.ActivePlayer.Id,
                    _ => true
                };
        }
    }
}
