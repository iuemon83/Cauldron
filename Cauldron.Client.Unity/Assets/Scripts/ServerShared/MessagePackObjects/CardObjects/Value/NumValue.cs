using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValue
    {
        public int? PureValue { get; }
        public NumValueCalculator NumValueCalculator { get; }
        public NumValueVariableCalculator NumValueVariableCalculator { get; }
        public NumValueModifier NumValueModifier { get; }

        public NumValue(
            int? PureValue = null,
            NumValueCalculator NumValueCalculator = null,
            NumValueVariableCalculator NumValueVariableCalculator = null,
            NumValueModifier NumValueModifier = null)
        {
            this.PureValue = PureValue;
            this.NumValueCalculator = NumValueCalculator;
            this.NumValueVariableCalculator = NumValueVariableCalculator;
            this.NumValueModifier = NumValueModifier;
        }
    }
}
