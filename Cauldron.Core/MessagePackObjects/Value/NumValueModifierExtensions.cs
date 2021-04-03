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
                NumValueModifier.ValueModifierOperator.Add => value + baseValue,
                NumValueModifier.ValueModifierOperator.Sub => value - baseValue,
                NumValueModifier.ValueModifierOperator.Multi => value * baseValue,
                NumValueModifier.ValueModifierOperator.Div => value / baseValue,
                NumValueModifier.ValueModifierOperator.Replace => baseValue,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
