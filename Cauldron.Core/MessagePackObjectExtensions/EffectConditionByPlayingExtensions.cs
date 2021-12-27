using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectConditionByPlayingExtensions
    {
        public static async ValueTask<bool> IsMatch(this EffectConditionByPlaying _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return await (_this.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true));
        }

        public static async ValueTask<bool> IsReserveEffect(this EffectConditionByPlaying _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return await (_this.If?.IsMatch(effectOwnerCard, eventArgs) ?? ValueTask.FromResult(true));
        }
    }
}
