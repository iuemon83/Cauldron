using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardSetCondition
    {
        public enum ConditionType
        {
            This,
            Other
        }

        public ConditionType Type { get; } = ConditionType.This;

        public TextCondition ValueCondition { get; }

        public CardSetCondition(ConditionType Type, TextCondition ValueCondition = null)
        {
            this.Type = Type;
            this.ValueCondition = ValueCondition;
        }
    }
}
