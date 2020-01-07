using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Server.Models
{
    public class CardFactory
    {
        private Dictionary<Guid, CardDef> CardDefListById { get; } = new Dictionary<Guid, CardDef>();

        private Dictionary<string, CardDef> CardDefListByFullName { get; } = new Dictionary<string, CardDef>();

        private Dictionary<Guid, Card> CardsById { get; } = new Dictionary<Guid, Card>();


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
                this.CardDefListByFullName.Add(cardDef.FullName, cardDef);
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
            return this.CardsById.TryGetValue(cardId, out var cardDef)
                ? cardDef
                : null;
        }

        public CardDef GetByFullName(string fullName)
        {
            return this.CardDefListByFullName.TryGetValue(fullName, out var cardDef)
                ? cardDef
                : null;
        }
    }
}
