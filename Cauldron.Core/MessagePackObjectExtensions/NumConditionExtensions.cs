using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class NumConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this NumCondition _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var x = await _this.Value.Calculate(effectOwnerCard, eventArgs);

            return await _this.Compare.IsMatch(x, effectOwnerCard, eventArgs);
        }
    }
}
