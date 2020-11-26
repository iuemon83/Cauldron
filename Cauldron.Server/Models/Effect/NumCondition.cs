using System;

namespace Cauldron.Server.Models.Effect
{
    public class NumCondition
    {
        public enum ConditionCompare
        {
            Equality,
            LessThan,
            GreaterTan,
        }

        public int Value { get; set; }
        public ConditionCompare Compare { get; set; }
        public bool Not { get; set; }

        public bool IsMatch(int checkValue)
        {
            var result = this.Compare switch
            {
                ConditionCompare.Equality => checkValue == this.Value,
                ConditionCompare.LessThan => checkValue <= this.Value,
                ConditionCompare.GreaterTan => checkValue >= this.Value,
                _ => throw new InvalidOperationException($"不正な入力値です: {this.Compare}")
            };

            return this.Not ? !result : result;
        }
    }
}
