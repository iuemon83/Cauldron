using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class Field
    {
        private RuleBook RuleBook { get; }

        private ConcurrentDictionary<CardId, Card> CardsById { get; } = new();

        public IReadOnlyList<Card> AllCards => this.CardsById.Values.ToArray();

        public bool Full => this.CardsById.Count >= this.RuleBook.MaxNumFieldCars;

        public Field(RuleBook ruleBook)
        {
            this.RuleBook = ruleBook;
        }

        public void Add(Card card)
        {
            if (this.Full)
            {
                return;
            }

            this.CardsById.TryAdd(card.Id, card);
        }

        public void Remove(Card card)
        {
            this.CardsById.TryRemove(card.Id, out _);
        }

        public Card GetById(CardId cardId)
        {
            return this.CardsById[cardId];
        }
    }
}
