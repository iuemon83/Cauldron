using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionModifyCardContext(IReadOnlyList<Card> ModifiedCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfModifyCard.TypeValue type)
            => type switch
            {
                ActionContextCardsOfModifyCard.TypeValue.Modified => this.ModifiedCards,
                _ => Array.Empty<Card>()
            };
    }
}
