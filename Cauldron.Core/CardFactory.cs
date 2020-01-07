using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core
{
    public class CardFactory
    {
        private Dictionary<Guid, CardDef> CardDefListById { get; } = new Dictionary<Guid, CardDef>();

        private Dictionary<Guid, Card> CardsById { get; } = new Dictionary<Guid, Card>();

        public CardFactory()
        {
        }

        public IEnumerable<CardDef> CardPool => this.CardDefListById.Values;

        public void SetCardPool(IEnumerable<CardDef> cardDefList)
        {
            foreach (var cardDef in cardDefList)
            {
                if (cardDef.Type == CardType.Unknown)
                {
                    throw new InvalidOperationException($"Card Type: {cardDef.Type}");
                }

                this.CardDefListById.Add(cardDef.Id, cardDef);
            }
        }

        public Card CreateNewRandom()
        {
            var classId = CardDefListById.Keys.ElementAt(Program.Random.Next(0, CardDefListById.Count));
            return this.CreateNew(classId);
        }

        public Card CreateNew(Guid classId)
        {
            var card = new Card(CardDefListById[classId]);
            this.CardsById.Add(card.Id, card);

            return card;
        }

        public Card GetById(Guid cardId)
        {
            return this.CardsById.TryGetValue(cardId, out var card)
                ? card
                : null;
        }
    }
}
