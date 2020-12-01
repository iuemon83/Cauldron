using System;

namespace Cauldron.Server.Models.Effect
{
    public class ValueModifier
    {
        public enum ValueModifierOperator
        {
            Add,
            Sub,
            Multi,
            Div,
        }

        public ValueModifierOperator Operator { get; set; }
        public int Value { get; set; }

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
