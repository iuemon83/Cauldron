using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionDrawCardContext(IReadOnlyList<Card> DrawnCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfDrawCard.TypeValue type)
            => type switch
            {
                ActionContextCardsOfDrawCard.TypeValue.DrawnCards => this.DrawnCards,
                _ => Array.Empty<Card>()
            };
    }
}
