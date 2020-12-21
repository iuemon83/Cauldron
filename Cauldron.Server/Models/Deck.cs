using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class Deck
    {
        private ConcurrentQueue<Card> Cards { get; set; }

        public int Count => this.Cards.Count;

        public Deck(IEnumerable<Card> cards)
        {
            this.Cards = new ConcurrentQueue<Card>(cards);
        }

        public void Remove(Card card)
        {
            if (!this.Cards.Contains(card)) return;

            this.Cards = new ConcurrentQueue<Card>(this.Cards.Where(c => c.Id != card.Id));
        }

        public Card Draw()
        {
            if (!this.Cards.Any()) return null;

            if (!this.Cards.TryDequeue(out var card))
            {
                throw new System.Exception("デッキからデキューに失敗!!!!!!!!!!!!!!");
            }

            return card;
        }
    }
}
