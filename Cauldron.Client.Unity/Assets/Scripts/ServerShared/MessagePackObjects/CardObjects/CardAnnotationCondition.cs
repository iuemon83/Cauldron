using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardAnnotationCondition
    {
        public string Value { get; }
        public bool Not { get; }

        public CardAnnotationCondition(string Value, bool Not = false)
        {
            this.Value = Value;
            this.Not = Not;
        }
    }
}
