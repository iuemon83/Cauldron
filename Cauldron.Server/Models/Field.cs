using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class Field
    {
        private RuleBook RuleBook { get; }

        private Dictionary<Guid, Card> CardsById { get; }

        public IReadOnlyList<Card> AllCards => this.CardsById.Values.ToArray();

        public bool Full => this.CardsById.Count >= this.RuleBook.MaxNumFieldCars;

        public Field(RuleBook ruleBook)
        {
            this.RuleBook = ruleBook;
            this.CardsById = new Dictionary<Guid, Card>();
        }

        public void Add(Card card)
        {
            if (this.Full)
            {
                return;
            }

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
    }
}
