using System;

namespace Cauldron.Server.Models.Effect.Value
{
    public record NumValueModifier(NumValueModifier.ValueModifierOperator Operator, NumValue Value)
    {
        public enum ValueModifierOperator
        {
            Add,
            Sub,
            Multi,
            Div,
            Replace,
        }

        public int Modify(Card effectOwnerCard, EffectEventArgs effectEventArgs, int value)
        {
            var baseValue = this.Value.Calculate(effectOwnerCard, effectEventArgs);

            return this.Operator switch
            {
                ValueModifierOperator.Add => value + baseValue,
                ValueModifierOperator.Sub => value - baseValue,
                ValueModifierOperator.Multi => value * baseValue,
                ValueModifierOperator.Div => value / baseValue,
                ValueModifierOperator.Replace => baseValue,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
