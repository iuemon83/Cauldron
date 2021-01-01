using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Cauldron.Server.Models
{
    public class CardFactory
    {
        private ConcurrentDictionary<CardDefId, CardDef> CardDefListById { get; } = new();

        private ConcurrentDictionary<CardId, Card> CardsById { get; } = new();

        private readonly RuleBook ruleBook;

        public IEnumerable<Card> GetAllCards => this.CardsById.Values;

        public IEnumerable<CardDef> CardPool => this.CardDefListById.Values;

        public CardFactory(RuleBook ruleBook)
        {
            this.ruleBook = ruleBook;
        }

        public void SetCardPool(IEnumerable<CardDef> cardDefList)
        {
            foreach (var cardDef in cardDefList)
            {
                if (cardDef.Type == CardType.Unknown)
                {
                    throw new InvalidOperationException($"Card Type: {cardDef.Type}");
                }

                cardDef.NumTurnsToCanAttack ??= this.ruleBook.DefaultNumTurnsToCanAttack;
                cardDef.NumAttacksLimitInTurn ??= this.ruleBook.DefaultNumAttacksLimitInTurn;

                this.CardDefListById.TryAdd(cardDef.Id, cardDef);
            }
        }

        public Card CreateNew(CardDefId cardDefId)
        {
            var card = new Card(CardDefListById[cardDefId]);
            this.CardsById.TryAdd(card.Id, card);

            return card;
        }

        public Card GetById(CardId cardId)
        {
            return this.CardsById.TryGetValue(cardId, out var cardDef)
                ? cardDef
                : null;
        }
    }
}
