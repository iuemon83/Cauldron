using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities
{
    public class Field
    {
        /// <summary>
        /// 順序を保存するため
        /// </summary>
        private Card?[] Cards { get; }

        public IReadOnlyList<Card> AllCards => this.Cards.OfType<Card>().ToArray();

        public IReadOnlyList<Card?> AllCardsWithIndex => this.Cards;

        public int Count => this.AllCards.Count;

        public bool Full => this.Cards.All(c => c != null);

        public Field(RuleBook ruleBook)
        {
            this.Cards = new Card[ruleBook.MaxNumFieldCards];
        }

        public int Add(Card card)
        {
            if (this.Full)
            {
                return -1;
            }

            foreach (var i in Enumerable.Range(0, this.Cards.Length))
            {
                if (this.Cards[i] == null)
                {
                    this.Cards[i] = card;
                    return i;
                }
            }

            return -1;
        }

        public void Remove(Card card)
        {
            foreach (var i in Enumerable.Range(0, this.Cards.Length))
            {
                if (this.Cards[i]?.Id == card.Id)
                {
                    this.Cards[i] = null;
                    return;
                }
            }
        }
    }
}
