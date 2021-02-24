using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class TextValue
    {
        public string PureValue { get; }
        public TextValueCalculator TextValueCalculator { get; }

        public TextValue(
            string PureValue = null,
            TextValueCalculator TextValueCalculator = null
            )
        {
            this.PureValue = PureValue;
            this.TextValueCalculator = TextValueCalculator;
        }
    }
}
