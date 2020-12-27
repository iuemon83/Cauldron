using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class Hands
    {
        private ConcurrentDictionary<CardId, Card> CardsById { get; } = new();

        public IReadOnlyList<Card> AllCards => this.CardsById.Values.ToArray();

        public int Count => this.CardsById.Count;

        public void Add(Card card)
        {
            this.CardsById.TryAdd(card.Id, card);
        }

        public void Remove(Card card)
        {
            this.CardsById.TryRemove(card.Id, out _);
        }

        public (bool, Card) TryGetById(CardId cardId)
        {
            var success = this.CardsById.TryGetValue(cardId, out var card);
            return (success, card);
        }

        public Card At(int index)
        {
            return this.CardsById.Values.ElementAt(index);
        }
    }
}
