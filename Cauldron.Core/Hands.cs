using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core
{
    public class Hands
    {
        private Dictionary<Guid, Card> CardsById { get; }

        public IReadOnlyList<Card> AllCards => this.CardsById.Values.ToArray();

        public int Count => this.CardsById.Count;

        public Hands()
        {
            this.CardsById = new Dictionary<Guid, Card>();
        }

        public void Add(Card card)
        {
            this.CardsById.Add(card.Id, card);
        }

        public void Remove(Card card)
        {
            this.CardsById.Remove(card.Id);
        }

        public Card GetById(Guid cardId)
        {
            return this.CardsById[cardId];
        }

        public Card At(int index)
        {
            return this.CardsById.Values.ElementAt(index);
        }
    }
}
