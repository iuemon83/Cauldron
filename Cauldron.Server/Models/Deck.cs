using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class Deck
    {
        private Queue<Card> cards;

        public int Count => this.cards.Count;

        public Deck(IEnumerable<Card> cards)
        {
            this.cards = new(cards);
        }

        public void Remove(Card card)
        {
            if (!this.cards.Contains(card)) return;

            this.cards = new(this.cards.Where(c => c.Id != card.Id));
        }

        public Card Draw()
        {
            if (!this.cards.Any()) return null;

            if (!this.cards.TryDequeue(out var card))
            {
                throw new System.Exception("デッキからデキューに失敗!!!!!!!!!!!!!!");
            }

            return card;
        }

        public void Shuffle()
        {
            this.cards = new(this.cards.OrderBy(_ => Guid.NewGuid()));
        }
    }
}
