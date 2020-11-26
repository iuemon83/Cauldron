using System;

namespace Cauldron.Server.Models.Effect
{
    public class TextCondition
    {
        public enum ConditionCompare
        {
            Equality,
            Like,
        }

        public string Value { get; set; }
        public ConditionCompare Compare { get; set; }
        public bool Not { get; set; }

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
