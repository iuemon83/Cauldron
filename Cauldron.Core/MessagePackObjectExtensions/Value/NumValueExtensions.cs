using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueExtensions
    {
        public static async ValueTask<int> Calculate(this NumValue numValue, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            async ValueTask<int> CalcBaseValue()
            {
                if (numValue.PureValue != null)
                {
                    return numValue.PureValue.Value;
                }
                else if (numValue.NumValueCalculator != null)
                {
                    return await numValue.NumValueCalculator.Calculate(effectOwnerCard, effectEventArgs);
                }
                else if (numValue.NumValueVariableCalculator != null)
                {
                    return numValue.NumValueVariableCalculator.Calculate(effectOwnerCard, effectEventArgs);
                }
                else
                {
                    return 0;
                }
            }

            var baseValue = await CalcBaseValue();

            return await (numValue.NumValueModifier?.Modify(effectOwnerCard, effectEventArgs, baseValue)
                ?? ValueTask.FromResult(baseValue));
        }
    }
}
