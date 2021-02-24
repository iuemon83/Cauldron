using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class ZoneValue
    {
        public ZonePrettyName[] PureValue { get; set; }
        public ZoneValueCalculator ZoneValueCalculator { get; set; }

        public ZoneValue(ZonePrettyName[] PureValue = null, ZoneValueCalculator ZoneValueCalculator = null)
        {
            this.PureValue = PureValue;
            this.ZoneValueCalculator = ZoneValueCalculator;
        }
    }
}
