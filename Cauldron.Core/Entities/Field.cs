using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;

namespace Cauldron.Core.Entities
{
    public class Field
    {
        private RuleBook RuleBook { get; }

        /// <summary>
        /// 順序を保存するため
        /// </summary>
        private List<Card> Cards { get; } = new();

        public IReadOnlyList<Card> AllCards => this.Cards;

        public int Count => this.AllCards.Count;

        public bool Full => this.Count >= this.RuleBook.MaxNumFieldCars;

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

            this.Cards.Add(card);
        }

        public void Remove(Card card)
        {
            this.Cards.Remove(card);
        }
    }
}
