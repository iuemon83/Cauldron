#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValue
    {
        public int? PureValue { get; }
        public NumValueCalculator? NumValueCalculator { get; }
        public NumVariable? NumVariable { get; }
        public NumValueModifier? NumValueModifier { get; }

        public NumValue(
            int? PureValue = null,
            NumValueCalculator? NumValueCalculator = null,
            NumVariable? NumVariable = null,
            NumValueModifier? NumValueModifier = null)
        {
            this.PureValue = PureValue;
            this.NumValueCalculator = NumValueCalculator;
            this.NumVariable = NumVariable;
            this.NumValueModifier = NumValueModifier;
        }
    }
}
