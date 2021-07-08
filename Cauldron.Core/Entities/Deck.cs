using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core.Entities
{
    public class Deck
    {
        private Queue<Card> cards;

        public int Count => this.cards.Count;

        public IReadOnlyList<Card> AllCards => this.cards.ToArray();

        public Deck(IEnumerable<Card> cards)
        {
            this.cards = new(cards);
        }

        public void Remove(Card card)
        {
            if (!this.cards.Contains(card)) return;

            this.cards = new(this.cards.Where(c => c.Id != card.Id));
        }

        public (bool, Card) TryDraw()
        {
            if (!this.cards.Any()) return (false, default);

            if (!this.cards.TryDequeue(out var card))
            {
                throw new Exception("デッキからデキューに失敗!!!!!!!!!!!!!!");
            }

            return (true, card);
        }

        public void Shuffle()
        {
            this.cards = new(this.cards.OrderBy(_ => Guid.NewGuid()));
        }

        public void Insert(int index, Card card)
        {
            var list = this.cards.ToList();
            list.Insert(index, card);

            this.cards = new(list);
        }
    }
}
