using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities
{
    public class Hands
    {
        /// <summary>
        /// 順序を保存するため
        /// </summary>
        private List<Card> Cards { get; }

        public IReadOnlyList<Card> AllCards => this.Cards;

        public int Count => this.AllCards.Count;

        public Hands(RuleBook ruleBook)
        {
            this.Cards = new List<Card>(ruleBook.MaxNumHands);
        }

        public void Add(Card card)
        {
            this.Cards.Add(card);
        }

        public void Remove(Card card)
        {
            this.Cards.Remove(card);
        }

        public int Index(Card card)
        {
            return this.Cards.IndexOf(card);
        }
    }
}
