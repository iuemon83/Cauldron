using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionMoveCardContext(IReadOnlyList<Card> MovedCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfMoveCard.TypeValue type)
            => type switch
            {
                ActionContextCardsOfMoveCard.TypeValue.Moved => this.MovedCards,
                _ => Array.Empty<Card>()
            };

        public int GetNum() => 0;

        public string GetText() => "";
    }
}
