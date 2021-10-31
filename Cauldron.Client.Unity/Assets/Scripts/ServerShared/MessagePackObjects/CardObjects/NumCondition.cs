using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class NumCondition
    {
        public NumValue Value { get; }

        public NumCompare Compare { get; }

        public bool Not { get; }

        public NumCondition(
            NumValue Value,
            NumCompare Compare,
            bool Not = false
            )
        {
            this.Value = Value;
            this.Compare = Compare;
            this.Not = Not;
        }
    }
}
