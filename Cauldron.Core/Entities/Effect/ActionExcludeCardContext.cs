using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionExcludeCardContext(IReadOnlyList<Card> ExcludedCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfExcludeCard.TypeValue type)
            => type switch
            {
                ActionContextCardsOfExcludeCard.TypeValue.Excluded => this.ExcludedCards,
                _ => Array.Empty<Card>()
            };
    }
}
