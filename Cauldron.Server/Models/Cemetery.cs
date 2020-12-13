using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    /// <summary>
    /// 墓地
    /// </summary>
    public class Cemetery
    {
        private Dictionary<Guid, Card> CardsById { get; } = new();

        public IReadOnlyList<Card> AllCards => this.CardsById.Values.ToArray();

        public void Add(Card card)
        {
            this.CardsById.Add(card.Id, card);
        }
    }
}
