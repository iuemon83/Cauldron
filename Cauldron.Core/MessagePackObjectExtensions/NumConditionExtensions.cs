using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class NumConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this NumCondition _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var x = await _this.Value.Calculate(effectOwnerCard, eventArgs);

            return _this.Compare.IsMatch(x);
        }
    }
}
