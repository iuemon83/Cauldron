using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectCondition _this, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return _this.Zone.IsMatchedZone(effectOwnerCard, eventArgs)
                && (_this.While?.IsMatch(effectOwnerCard, eventArgs) ?? true)
                && (await (_this.When?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)))
                && (await (_this.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true)));
        }

        private static bool IsMatchedZone(this ZonePrettyName _this, Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (_this == ZonePrettyName.Any)
            {
                return true;
            }

            var opponentId = eventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id;
            var (success, zone) = _this.TryGetZone(effectOwnerCard.OwnerId, opponentId, effectOwnerCard.OwnerId);
            return success && effectOwnerCard.Zone == zone;
        }
    }
}
