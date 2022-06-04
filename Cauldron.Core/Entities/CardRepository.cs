using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Concurrent;

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

        public void SetCardPool(CardPool cardPool)
        {
            foreach (var cardset in cardPool.cardSets)
            {
                foreach (var cardDef in cardset.Cards)
                {
                    cardDef.CardSetName = cardset.Name;
                    cardDef.NumTurnsToCanAttackToCreature ??= this.ruleBook.DefaultNumTurnsToCanAttack;
                    cardDef.NumTurnsToCanAttackToPlayer ??= this.ruleBook.DefaultNumTurnsToCanAttack;
                    cardDef.NumAttacksLimitInTurn ??= this.ruleBook.DefaultNumAttacksLimitInTurn;
                    cardDef.LimitNumCardsInDeck ??= cardDef.IsToken ? 0 : this.ruleBook.DefaultLimitNumCardsInDeck;

                    this.CardDefListById.TryAdd(cardDef.Id, cardDef);
                }
            }
        }

        public Card? CreateNew(CardDefId cardDefId)
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
            return (exists, card ?? Card.Empty);
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
            return (exists, card ?? CardDef.Empty);
        }

        public bool Remove(Card cardToExclude)
        {
            return this.CardsById.Remove(cardToExclude.Id, out var _);
        }
    }
}
