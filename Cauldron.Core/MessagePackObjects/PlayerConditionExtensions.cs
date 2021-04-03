using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class PlayerConditionExtensions
    {
        public static bool IsMatch(this PlayerCondition playerCondition, Card effectOwnerCard, Player player, EffectEventArgs eventArgs)
        {
            return
                playerCondition.Context switch
                {
                    PlayerCondition.PlayerConditionContext.EventSource => player.Id == eventArgs.SourcePlayer.Id,
                    _ => true
                }
                && playerCondition.Type switch
                {
                    PlayerCondition.PlayerConditionType.You => player.Id == effectOwnerCard.OwnerId,
                    PlayerCondition.PlayerConditionType.Opponent => player.Id != effectOwnerCard.OwnerId,
                    PlayerCondition.PlayerConditionType.Active => player.Id == eventArgs.GameMaster.ActivePlayer.Id,
                    PlayerCondition.PlayerConditionType.NonActive => player.Id != eventArgs.GameMaster.ActivePlayer.Id,
                    _ => true
                };
        }
    }
}
