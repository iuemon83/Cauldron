using Cauldron.Core.Entities.Effect;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class ZoneValueExtensions
    {
        public static async ValueTask<ZonePrettyName[]> Calculate(this ZoneValue zoneValue, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            async ValueTask<ZonePrettyName[]> FromValueCalculator()
            {
                if (zoneValue.ZoneValueCalculator == null)
                {
                    return null;
                }

                var zones = await zoneValue.ZoneValueCalculator.Calculate(effectOwnerCard, effectEventArgs);
                return zones.Select(zone => zone.AsZonePrettyName(effectOwnerCard)).ToArray();
            }

            return zoneValue.PureValue
                ?? await FromValueCalculator()
                ?? Array.Empty<ZonePrettyName>();
        }
    }
}
