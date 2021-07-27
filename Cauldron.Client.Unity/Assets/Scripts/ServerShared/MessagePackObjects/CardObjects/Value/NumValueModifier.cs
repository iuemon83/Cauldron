using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueModifier
    {
        public enum OperatorValue
        {
            [DisplayText("＋")]
            Add,
            [DisplayText("－")]
            Sub,
            [DisplayText("×")]
            Multi,
            [DisplayText("÷")]
            Div,
            [DisplayText("置換")]
            Replace,
        }

        public OperatorValue Operator { get; }
        public NumValue Value { get; }

        public NumValueModifier(
            OperatorValue Operator,
            NumValue Value
            )
        {
            this.Operator = Operator;
            this.Value = Value;
        }
    }
}
