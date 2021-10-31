using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ConditionNotExtensions
    {
        public static async ValueTask<bool> IsMatch(this ConditionNot _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (_this.Condition == default)
            {
                return true;
            }

            var x = await _this.Condition.IsMatch(effectOwnerCard, eventArgs);

            return !x;
        }
    }
}
