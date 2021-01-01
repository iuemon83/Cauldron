using System.Collections.Generic;

namespace Cauldron.Server.Models
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
        private Dictionary<CardId, Card> CardsById { get; } = new();

        public IReadOnlyList<Card> AllCards => this.Cards;

        public void Add(Card card)
        {
            this.Cards.Add(card);
            this.CardsById.Add(card.Id, card);
        }
    }
}
