using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectConditionExtensions
    {
        public static async ValueTask<bool> IsMatchAnyZone(this EffectCondition _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return (await (_this.While?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)))
                && (await (_this.When?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)))
                && (await (_this.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)));
        }

        public static async ValueTask<bool> IsMatchOnPlay(this EffectCondition _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return _this.Zone == ZonePrettyName.None
                && _this.When == default
                && _this.While == default
                && (await (_this.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)));
        }

        public static async ValueTask<bool> IsMatch(this EffectCondition _this, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return _this.IsMatchedZone(effectOwnerCard, eventArgs)
                && (await (_this.While?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)))
                && (await (_this.When?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)))
                && (await (_this.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)));
        }

        private static bool IsMatchedZone(this EffectCondition _this, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var opponentId = eventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id;
            var (success, zone) = _this.Zone.TryGetZone(effectOwnerCard.OwnerId, opponentId, effectOwnerCard.OwnerId);
            return success && effectOwnerCard.Zone == zone;
        }
    }
}
