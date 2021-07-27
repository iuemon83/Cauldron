using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class NumCondition
    {
        public enum CompareValue
        {
            [DisplayText("等しい")]
            Equality,
            [DisplayText("以下")]
            LessThan,
            [DisplayText("以上")]
            GreaterThan,
        }

        public int Value { get; }
        public NumCondition.CompareValue Compare { get; }
        public bool Not { get; }

        public NumCondition(
            int Value,
            NumCondition.CompareValue Compare,
            bool not = false
            )
        {
            this.Value = Value;
            this.Compare = Compare;
            this.Not = not;
        }
    }
}
