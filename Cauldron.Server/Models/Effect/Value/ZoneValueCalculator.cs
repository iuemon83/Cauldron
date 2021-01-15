using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models.Effect.Value
{
    public record ZoneValueCalculator(Choice CardsChoice)
    {
        public IEnumerable<Zone> Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return effectEventArgs.GameMaster
                .ChoiceCards(effectOwnerCard, this.CardsChoice, effectEventArgs)
                .CardList
                .Select(c => c.Zone);
        }
    }
}
