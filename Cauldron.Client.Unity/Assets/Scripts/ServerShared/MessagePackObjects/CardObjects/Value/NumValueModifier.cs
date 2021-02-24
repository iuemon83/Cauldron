using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueModifier
    {
        public enum ValueModifierOperator
        {
            Add,
            Sub,
            Multi,
            Div,
            Replace,
        }

        public NumValueModifier.ValueModifierOperator Operator { get; set; }
        public NumValue Value { get; set; }

        public NumValueModifier(
            NumValueModifier.ValueModifierOperator Operator,
            NumValue Value
            )
        {
            this.Operator = Operator;
            this.Value = Value;
        }
    }
}
