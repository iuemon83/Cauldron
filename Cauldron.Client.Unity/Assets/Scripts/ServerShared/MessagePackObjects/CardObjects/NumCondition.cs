using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class NumCondition
    {
        public enum ConditionCompare
        {
            Equality,
            LessThan,
            GreaterThan,
        }

        public int Value { get; }
        public NumCondition.ConditionCompare Compare { get; }
        public bool Not { get; }

        public NumCondition(
            int Value,
            NumCondition.ConditionCompare Compare,
            bool not = false
            )
        {
            this.Value = Value;
            this.Compare = Compare;
            this.Not = not;
        }
    }
}
