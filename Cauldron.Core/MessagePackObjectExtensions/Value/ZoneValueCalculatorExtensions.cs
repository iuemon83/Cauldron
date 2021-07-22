using Cauldron.Core.Entities.Effect;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class ZoneValueCalculatorExtensions
    {
        public static async ValueTask<IEnumerable<Zone>> Calculate(this ZoneValueCalculator zoneValueCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster
                .Choice(effectOwnerCard, zoneValueCalculator.CardsChoice, effectEventArgs);

            return choiceResult.CardList.Select(c => c.Zone);
        }
    }
}
