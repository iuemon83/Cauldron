using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class ZoneValue
    {
        public ZonePrettyName[] PureValue { get; }
        public ZoneValueCalculator ZoneValueCalculator { get; }

        public ZoneValue(ZonePrettyName[] PureValue = null, ZoneValueCalculator ZoneValueCalculator = null)
        {
            this.PureValue = PureValue;
            this.ZoneValueCalculator = ZoneValueCalculator;
        }
    }
}
