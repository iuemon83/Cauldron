using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueCalculatorExtensions
    {
        public static async ValueTask<int> Calculate(this NumValueCalculator _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            if (_this.ForCard != null)
            {
                return await _this.ForCard.Calculate(effectOwnerCard, effectEventArgs);
            }

            if (_this.ForCounter != null)
            {
                return await _this.ForCounter.Calculate(effectOwnerCard, effectEventArgs);
            }

            return 0;
        }
    }
}
