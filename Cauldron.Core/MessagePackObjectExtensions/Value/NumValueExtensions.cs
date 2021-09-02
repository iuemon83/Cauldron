using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueExtensions
    {
        public static async ValueTask<int> Calculate(this NumValue _this, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            async ValueTask<int> CalcBaseValue()
            {
                if (_this.PureValue != null)
                {
                    return _this.PureValue.Value;
                }
                else if (_this.NumValueCalculator != null)
                {
                    return await _this.NumValueCalculator.Calculate(effectOwnerCard, effectEventArgs);
                }
                else if (_this.NumVariable != null)
                {
                    return _this.NumVariable.Calculate(effectOwnerCard, effectEventArgs);
                }
                else
                {
                    return 0;
                }
            }

            var baseValue = await CalcBaseValue();

            return await (_this.NumValueModifier?.Modify(effectOwnerCard, effectEventArgs, baseValue)
                ?? ValueTask.FromResult(baseValue));
        }
    }
}
