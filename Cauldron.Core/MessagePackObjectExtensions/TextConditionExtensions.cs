using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class TextConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this TextCondition _this,
            Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var x = await _this.Value.Calculate(effectOwnerCard, eventArgs);

            return await _this.Compare.IsMatch(effectOwnerCard, eventArgs, x);
        }
    }
}
