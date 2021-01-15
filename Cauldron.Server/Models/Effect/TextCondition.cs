using Cauldron.Server.Models.Effect.Value;
using System;

namespace Cauldron.Server.Models.Effect
{
    public record TextCondition(TextValue Value, TextCondition.ConditionCompare Compare, bool Not = false)
    {
        public enum ConditionCompare
        {
            Equality,
            Like,
        }

        public bool IsMatch(Card effectOwnerCard, EffectEventArgs effectEventArgs, string checkValue)
        {
            var value = this.Value.Calculate(effectOwnerCard, effectEventArgs);

            var result = this.Compare switch
            {
                ConditionCompare.Equality => checkValue == value,
                ConditionCompare.Like => checkValue.Contains(value),
                _ => throw new InvalidOperationException($"不正な入力値です: {this.Compare}")
            };

            return this.Not ? !result : result;
        }
    }
}
