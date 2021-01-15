using System;
using System.Linq;

namespace Cauldron.Server.Models.Effect.Value
{
    public record ZoneValue(
        ZonePrettyName[] PureValue = null,
        ZoneValueCalculator ZoneValueCalculator = null
        )
    {
        public ZonePrettyName[] Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return this.PureValue
                ?? this.ZoneValueCalculator?.Calculate(effectOwnerCard, effectEventArgs).Select(zone => zone.AsZonePrettyName(effectOwnerCard)).ToArray()
                ?? Array.Empty<ZonePrettyName>();
        }
    }
}
