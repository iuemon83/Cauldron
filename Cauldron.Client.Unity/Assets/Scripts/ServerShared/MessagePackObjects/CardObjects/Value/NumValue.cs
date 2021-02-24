using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValue
    {
        public int? PureValue { get; }
        public NumValueCalculator NumValueCalculator { get; set; }
        public NumValueVariableCalculator NumValueVariableCalculator { get; set; }
        public NumValueModifier NumValueModifier { get; set; }

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
