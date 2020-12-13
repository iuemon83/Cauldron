using System;

namespace Cauldron.Server.Models.Effect
{
    public record ValueModifier(ValueModifier.ValueModifierOperator Operator, int Value)
    {
        public enum ValueModifierOperator
        {
            Add,
            Sub,
            Multi,
            Div,
        }

        public int Modify(int value)
        {
            return this.Operator switch
            {
                ValueModifierOperator.Add => value + this.Value,
                ValueModifierOperator.Sub => value - this.Value,
                ValueModifierOperator.Multi => value * this.Value,
                ValueModifierOperator.Div => value / this.Value,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
