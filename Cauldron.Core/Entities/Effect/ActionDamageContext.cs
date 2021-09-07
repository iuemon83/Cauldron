using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionDamageContext(IReadOnlyList<Card> DamagedCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfDamage.TypeValue type)
            => type switch
            {
                ActionContextCardsOfDamage.TypeValue.DamagedCards => this.DamagedCards,
                _ => Array.Empty<Card>()
            };
    }
}
