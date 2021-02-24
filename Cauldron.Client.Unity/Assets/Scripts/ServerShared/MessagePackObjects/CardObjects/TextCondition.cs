using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class TextCondition
    {
        public enum ConditionCompare
        {
            Equality,
            Like,
        }

        public TextValue Value { get; }
        public TextCondition.ConditionCompare Compare { get; }
        public bool Not { get; }

        public TextCondition(
            TextValue Value,
            TextCondition.ConditionCompare Compare,
            bool not = false
            )
        {
            this.Value = Value;
            this.Compare = Compare;
            this.Not = not;
        }
    }
}
