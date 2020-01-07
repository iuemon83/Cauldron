using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class Deck
    {
        private Queue<Card> Cards { get; }

        public int Count => this.Cards.Count;

        public Deck(IEnumerable<Card> cards)
        {
            this.Cards = new Queue<Card>(cards);
        }
        public Card Draw()
        {
            if (!this.Cards.Any()) return null;

            return this.Cards.Dequeue();
        }
    }
}
