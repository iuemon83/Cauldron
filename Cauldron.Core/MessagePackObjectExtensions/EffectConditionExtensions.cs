using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectConditionExtensions
    {
        public static async ValueTask<bool> IsMatchAnyZone(this EffectCondition effectCondition, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return (await (effectCondition.While?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)))
                && await effectCondition.When.IsMatch(effectOwnerCard, eventArgs)
                && (await (effectCondition.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)));
        }

        public static async ValueTask<bool> IsMatch(this EffectCondition effectCondition, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return effectCondition.IsMatchedZone(effectOwnerCard, eventArgs)
                && (await (effectCondition.While?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)))
                && await effectCondition.When.IsMatch(effectOwnerCard, eventArgs)
                && (await (effectCondition.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)));
        }

        private static bool IsMatchedZone(this EffectCondition effectCondition, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var opponentId = eventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id;
            var (success, zone) = effectCondition.ZonePrettyName.TryGetZone(effectOwnerCard.OwnerId, opponentId);
            return success && effectOwnerCard.Zone == zone;
        }
    }
}
