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

        //private Dictionary<CardId, Card> CardsById { get; } = new();

        public IReadOnlyList<Card> AllCards => this.Cards;

        public int Count => this.AllCards.Count;

        public void Add(Card card)
        {
            this.Cards.Add(card);
            //this.CardsById.Add(card.Id, card);
        }

        public void Remove(Card card)
        {
            this.Cards.Remove(card);
            //this.CardsById.Remove(card.Id);
        }

        //public (bool, Card) TryGetById(CardId cardId)
        //{
        //    var success = this.CardsById.TryGetValue(cardId, out var card);
        //    return (success, card);
        //}
    }
}
