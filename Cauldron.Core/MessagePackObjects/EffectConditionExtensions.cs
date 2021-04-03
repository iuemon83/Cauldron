using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectCondition effectCondition, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return effectCondition.IsMatchedZone(effectOwnerCard, eventArgs)
                && (await (effectCondition.While?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)))
                && await effectCondition.When.IsMatch(effectOwnerCard, eventArgs)
                && (await (effectCondition.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)));
        }

        private static bool IsMatchedZone(this EffectCondition effectCondition, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var zone = effectCondition.ZonePrettyName.FromPrettyName(effectOwnerCard.OwnerId,
                eventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id);
            return effectOwnerCard.Zone == zone;
        }
    }
}
