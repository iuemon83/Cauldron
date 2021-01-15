using System.Collections.Generic;

namespace Cauldron.Server.Models
{
    public class Field
    {
        private RuleBook RuleBook { get; }

        /// <summary>
        /// 順序を保存するため
        /// </summary>
        private List<Card> Cards { get; } = new();
        private Dictionary<CardId, Card> CardsById { get; } = new();

        public IReadOnlyList<Card> AllCards => this.Cards;

        public int Count => this.AllCards.Count;

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

            this.Cards.Add(card);
            this.CardsById.Add(card.Id, card);
        }

        public void Remove(Card card)
        {
            this.Cards.Remove(card);
            this.CardsById.Remove(card.Id);
        }

        public Card GetById(CardId cardId)
        {
            return this.CardsById[cardId];
        }
    }
}
