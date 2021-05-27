using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class NumCondition
    {
        public enum ConditionCompare
        {
            [DisplayText("等しい")]
            Equality,
            [DisplayText("以下")]
            LessThan,
            [DisplayText("以上")]
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
