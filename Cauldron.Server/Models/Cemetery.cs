using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    /// <summary>
    /// 墓地
    /// </summary>
    public class Cemetery
    {
        private ConcurrentDictionary<Guid, Card> CardsById { get; } = new();

        public IReadOnlyList<Card> AllCards => this.CardsById.Values.ToArray();
        public int Count => this.AllCards.Count;

        public void Add(Card card)
        {
            this.CardsById.TryAdd(card.Id, card);
        }
    }
}
