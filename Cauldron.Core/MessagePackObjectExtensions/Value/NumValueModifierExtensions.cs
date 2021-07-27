using Cauldron.Core.Entities.Effect;
using System;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueModifierExtensions
    {
        public static async ValueTask<int> Modify(this NumValueModifier numValueModifier, Card effectOwnerCard, EffectEventArgs effectEventArgs, int value)
        {
            var baseValue = await numValueModifier.Value.Calculate(effectOwnerCard, effectEventArgs);

            return numValueModifier.Operator switch
            {
                NumValueModifier.OperatorValue.Add => value + baseValue,
                NumValueModifier.OperatorValue.Sub => value - baseValue,
                NumValueModifier.OperatorValue.Multi => value * baseValue,
                NumValueModifier.OperatorValue.Div => value / baseValue,
                NumValueModifier.OperatorValue.Replace => baseValue,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
