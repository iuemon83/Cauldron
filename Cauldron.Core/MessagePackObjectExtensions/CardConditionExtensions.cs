using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardConditionExtensions
    {
        public static async ValueTask<Card[]> ChoiceCandidateCards(this CardCondition cardCondition, Card effectOwnerCard, EffectEventArgs effectEventArgs, CardRepository cardRepository, PlayerRepository playerRepository)
        {
            var source = await cardCondition.ListMatchedCards(effectOwnerCard, effectEventArgs, cardRepository);

            if (cardCondition.ZoneCondition == null)
            {
                return source.ToArray();
            }

            var zonPrettyNames = await cardCondition.ZoneCondition.Value.Calculate(effectOwnerCard, effectEventArgs);
            var (exists, player) = playerRepository.TryGet(effectOwnerCard.OwnerId);

            var sourceByZone = zonPrettyNames
                .SelectMany(zoneType => zoneType switch
                {
                    ZonePrettyName.YouField => player.Field.AllCards,
                    ZonePrettyName.OpponentField => playerRepository.Opponents(effectOwnerCard.OwnerId)[0].Field.AllCards,
                    ZonePrettyName.YouHand => player.Hands.AllCards,
                    ZonePrettyName.OpponentHand => playerRepository.Opponents(effectOwnerCard.OwnerId)[0].Hands.AllCards,
                    ZonePrettyName.YouCemetery => player.Cemetery.AllCards,
                    ZonePrettyName.OpponentCemetery => playerRepository.Opponents(effectOwnerCard.OwnerId)[0].Cemetery.AllCards,
                    _ => Array.Empty<Card>(),
                })
                .ToArray();

            return source
                .Where(c => sourceByZone.Any(cz => cz.Id == c.Id))
                .ToArray();
        }

        public static async ValueTask<CardDef[]> ChoiceCandidateCardDefs(this CardCondition cardCondition,
            Card effectOwnerCard, EffectEventArgs effectEventArgs, CardRepository cardRepository)
        {
            if (cardCondition.ZoneCondition == null)
            {
                return Array.Empty<CardDef>();
            }

            var zonePrettyNames = await cardCondition.ZoneCondition.Value.Calculate(effectOwnerCard, effectEventArgs);

            async ValueTask<IEnumerable<CardDef>> GetMatchedCarddefs()
            {
                var carddefAndIsMatchTasks = cardRepository.CardPool
                    .Select(async cdef => (Carddef: cdef, IsMatch: await cardCondition.IsMatch(effectOwnerCard, effectEventArgs, cdef)));

                var carddefAndIsMatch = await Task.WhenAll(carddefAndIsMatchTasks);

                return carddefAndIsMatch
                    .Where(x => x.IsMatch)
                    .Select(x => x.Carddef);
            }

            var carddefsTasks = zonePrettyNames
                .Select(async zoneType => zoneType switch
                {
                    ZonePrettyName.CardPool => await GetMatchedCarddefs(),
                    _ => Array.Empty<CardDef>(),
                });

            var carddefs = await Task.WhenAll(carddefsTasks);

            return carddefs
                .SelectMany(c => c)
                .ToArray();
        }

        public static async ValueTask<Card[]> ListMatchedCards(this CardCondition cardCondition, Card effectOwnerCard, EffectEventArgs eventArgs, CardRepository cardRepository)
        {
            var source = cardCondition.ChoiceCandidateSourceCards(effectOwnerCard, eventArgs, cardRepository);

            var mathedCards = new List<Card>();
            foreach (var card in source)
            {
                var isMathed = await cardCondition.IsMatch(effectOwnerCard, eventArgs, card);

                if (isMathed)
                {
                    mathedCards.Add(card);
                }
            }

            return mathedCards.ToArray();
        }

        private static IEnumerable<Card> ChoiceCandidateSourceCards(this CardCondition cardCondition, Card effectOwnerCard, EffectEventArgs eventArgs, CardRepository cardRepository)
        {
            var actionContext = cardCondition?.ActionContext;
            if (actionContext != null)
            {
                return actionContext.GetCards(effectOwnerCard, eventArgs);
            }
            else
            {
                return cardRepository.AllCards;
            }
        }

        public static async ValueTask<bool> IsMatch(this CardCondition cardCondition, Card effectOwnerCard, EffectEventArgs effectEventArgs, Card cardToMatch)
        {
            return
                (cardCondition.Context switch
                {
                    CardCondition.CardConditionContext.This => cardToMatch.Id == effectOwnerCard.Id,
                    CardCondition.CardConditionContext.Others => cardToMatch.Id != effectOwnerCard.Id,
                    CardCondition.CardConditionContext.EventSource => cardToMatch.Id == effectEventArgs.SourceCard.Id,
                    CardCondition.CardConditionContext.DamageFrom => cardToMatch.Id == effectEventArgs.DamageContext.DamageSourceCard.Id,
                    CardCondition.CardConditionContext.DamageTo => cardToMatch.Id == effectEventArgs.DamageContext.GuardCard.Id,
                    _ => true
                })
                && (cardCondition.CostCondition?.IsMatch(cardToMatch.Cost) ?? true)
                && (cardCondition.PowerCondition?.IsMatch(cardToMatch.Power) ?? true)
                && (cardCondition.ToughnessCondition?.IsMatch(cardToMatch.Toughness) ?? true)
                && (await (cardCondition.CardSetCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch) ?? ValueTask.FromResult(true)))
                && (await (cardCondition.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch.Name) ?? ValueTask.FromResult(true)))
                && (cardCondition.TypeCondition?.IsMatch(cardToMatch.Type) ?? true)
                && (await (cardCondition.ZoneCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardToMatch.Zone) ?? ValueTask.FromResult(true)))
                ;
        }

        public static async ValueTask<bool> IsMatch(this CardCondition cardCondition, Card effectOwnerCard, EffectEventArgs effectEventArgs, CardDef cardDefToMatch)
        {
            return
                (cardCondition.CostCondition?.IsMatch(cardDefToMatch.Cost) ?? true)
                && (cardCondition.PowerCondition?.IsMatch(cardDefToMatch.Power) ?? true)
                && (cardCondition.ToughnessCondition?.IsMatch(cardDefToMatch.Toughness) ?? true)
                && (await (cardCondition.CardSetCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardDefToMatch) ?? ValueTask.FromResult(true)))
                && (await (cardCondition.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardDefToMatch.Name) ?? ValueTask.FromResult(true)))
                && (cardCondition.TypeCondition?.IsMatch(cardDefToMatch.Type) ?? true);
        }
    }
}
