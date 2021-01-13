using System;
using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    public record ActionMoveCardContext(IReadOnlyList<Card> MovedCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfMoveCard.ValueType type)
            => type switch
            {
                ActionContextCardsOfMoveCard.ValueType.Moved => this.MovedCards,
                _ => Array.Empty<Card>()
            };

        public int GetNum() => 0;

        public string GetText() => "";
    }
}
