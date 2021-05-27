using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class TextCondition
    {
        public enum ConditionCompare
        {
            [DisplayText("等しい")]
            Equality,
            [DisplayText("含む")]
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
