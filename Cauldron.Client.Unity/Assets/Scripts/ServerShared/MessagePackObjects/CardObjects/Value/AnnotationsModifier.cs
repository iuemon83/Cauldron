using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class AnnotationsModifier
    {
        public enum OperatorValue
        {
            [DisplayText("追加")]
            Add,
            [DisplayText("削除")]
            Remove,
            [DisplayText("全削除")]
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
