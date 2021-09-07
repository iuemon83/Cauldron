using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionDestroyCardContext(IReadOnlyList<Card> DestroyedCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfDestroyCard.TypeValue type)
            => type switch
            {
                ActionContextCardsOfDestroyCard.TypeValue.DestroyedCards => this.DestroyedCards,
                _ => Array.Empty<Card>()
            };
    }
}
