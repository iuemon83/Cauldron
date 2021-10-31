using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ConditionOrExtensions
    {
        public static async ValueTask<bool> IsMatch(this ConditionOr _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            foreach (var c in _this.Conditions)
            {
                var x = await c.IsMatch(effectOwnerCard, eventArgs);
                if (x)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
