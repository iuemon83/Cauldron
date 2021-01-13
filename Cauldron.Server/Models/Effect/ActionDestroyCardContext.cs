using System;
using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    public record ActionDestroyCardContext(IReadOnlyList<Card> DestroyedCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfDestroyCard.ValueType type)
            => type switch
            {
                ActionContextCardsOfDestroyCard.ValueType.Destroyed => this.DestroyedCards,
                _ => Array.Empty<Card>()
            };

        public int GetNum() => 0;

        public string GetText() => "";
    }
}
