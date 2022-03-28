using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities
{
    /// <summary>
    /// 墓地
    /// </summary>
    public class Cemetery
    {
        /// <summary>
        /// 順序を保存するため
        /// </summary>
        private List<Card> Cards { get; } = new();

        public IReadOnlyList<Card> AllCards => this.Cards;
        public int Count => this.AllCards.Count;

        public void Add(Card card)
        {
            card.Reset();
            this.Cards.Add(card);
        }

        public void Remove(Card card)
        {
            this.Cards.Remove(card);
        }
    }
}
