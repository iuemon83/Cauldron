using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core.Entities
{
    public class CardRepository
    {
        private ConcurrentDictionary<CardDefId, CardDef> CardDefListById { get; } = new();

        private ConcurrentDictionary<CardId, Card> CardsById { get; } = new();

        private readonly RuleBook ruleBook;

        public IReadOnlyList<Card> AllCards => this.CardsById.Values.ToArray();

        public IReadOnlyList<CardDef> CardPool => this.CardDefListById.Values.ToArray();

        public CardRepository(RuleBook ruleBook)
        {
            this.ruleBook = ruleBook;
        }

        public void SetCardPool(IEnumerable<CardSet> cardsetList)
        {
            foreach (var cardset in cardsetList)
            {
                foreach (var cardDef in cardset.Cards)
                {
                    cardDef.CardSetName = cardset.Name;
                    cardDef.NumTurnsToCanAttack ??= this.ruleBook.DefaultNumTurnsToCanAttack;
                    cardDef.NumAttacksLimitInTurn ??= this.ruleBook.DefaultNumAttacksLimitInTurn;

                    this.CardDefListById.TryAdd(cardDef.Id, cardDef);
                }
            }
        }

        public Card CreateNew(CardDefId cardDefId)
        {
            if (!this.CardDefListById.TryGetValue(cardDefId, out var cardDef))
            {
                return null;
            }

            var card = new Card(cardDef);
            this.CardsById.TryAdd(card.Id, card);

            return card;
        }

        public (bool, Card) TryGetById(CardId cardId)
        {
            var exists = this.CardsById.TryGetValue(cardId, out var card);
            return (exists, card);
        }

        public (bool, Card) TryGetById(CardId cardId, Zone zone)
        {
            var (exists, card) = this.TryGetById(cardId);

            var isMatched = exists
                && card.Zone.PlayerId == zone.PlayerId
                && card.Zone.ZoneName == zone.ZoneName;

            return (isMatched, card);
        }

        public (bool, CardDef) TryGetCardDefById(CardDefId carddefId)
        {
            var exists = this.CardDefListById.TryGetValue(carddefId, out var card);
            return (exists, card);
        }
    }
}
