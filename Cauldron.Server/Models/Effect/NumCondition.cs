using System;

namespace Cauldron.Server.Models.Effect
{
    public record NumCondition(int Value, NumCondition.ConditionCompare Compare, bool Not = false)
    {
        public enum ConditionCompare
        {
            Equality,
            LessThan,
            GreaterThan,
        }

        public bool IsMatch(int checkValue)
        {
            var result = this.Compare switch
            {
                ConditionCompare.Equality => checkValue == this.Value,
                ConditionCompare.LessThan => checkValue <= this.Value,
                ConditionCompare.GreaterThan => checkValue >= this.Value,
                _ => throw new InvalidOperationException($"{nameof(this.Compare)}: {this.Compare}")
            };

            return result ^ this.Not;
        }
    }
}
