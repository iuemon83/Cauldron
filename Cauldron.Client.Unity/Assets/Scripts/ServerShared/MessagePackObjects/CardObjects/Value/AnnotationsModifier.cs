using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class AnnotationsModifier
    {
        public enum OperatorValue
        {
            Add,
            Remove,
            Clear
        }

        public OperatorValue Operator { get; }
        public string[] Value { get; }

        public AnnotationsModifier(
            string[] Value,
            OperatorValue Operator
            )
        {
            this.Value = Value;
            this.Operator = Operator;
        }
    }
}
