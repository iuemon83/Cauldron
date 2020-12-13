﻿using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class Deck
    {
        private Queue<Card> Cards { get; set; }

        public int Count => this.Cards.Count;

        public Deck(IEnumerable<Card> cards)
        {
            this.Cards = new Queue<Card>(cards);
        }

        public void Remove(Card card)
        {
            if (!this.Cards.Contains(card)) return;

            this.Cards = new Queue<Card>(this.Cards.Where(c => c.Id != card.Id));
        }

        public Card Draw()
        {
            if (!this.Cards.Any()) return null;

            return this.Cards.Dequeue();
        }
    }
}
