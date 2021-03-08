using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;

namespace Cauldron.Core.Entities
{
    public partial class Hands
    {
        /// <summary>
        /// 順序を保存するため
        /// </summary>
        private List<Card> Cards { get; } = new();

        public IReadOnlyList<Card> AllCards => this.Cards;

        public int Count => this.AllCards.Count;

        public void Add(Card card)
        {
            this.Cards.Add(card);
        }

        public void Remove(Card card)
        {
            this.Cards.Remove(card);
        }
    }
}
