using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
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
