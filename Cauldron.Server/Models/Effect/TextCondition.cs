using System;

namespace Cauldron.Server.Models.Effect
{
    public record TextCondition(string Value, TextCondition.ConditionCompare Compare, bool Not = false)
    {
        public enum ConditionCompare
        {
            Equality,
            Like,
        }

        public bool IsMatch(string checkValue)
        {
            var result = this.Compare switch
            {
                ConditionCompare.Equality => checkValue == this.Value,
                ConditionCompare.Like => checkValue.Contains(this.Value),
                _ => throw new InvalidOperationException($"不正な入力値です: {this.Compare}")
            };

            return this.Not ? !result : result;
        }
    }
}
