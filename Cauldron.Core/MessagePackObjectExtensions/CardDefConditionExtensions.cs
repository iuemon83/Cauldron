using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardDefConditionExtensions
    {
        public static async ValueTask<CardDef[]> ListMatchedCardDefs(this CardDefCondition _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs, CardRepository cardRepository)
        {
            async ValueTask<IEnumerable<CardDef>> GetMatchedFromPool()
            {
                var carddefAndIsMatchTasks = cardRepository.CardPool
                    .Select(async cdef => (
                        Carddef: cdef,
                        IsMatch: await _this.IsMatch(cdef, effectOwnerCard, effectEventArgs)));

                var carddefAndIsMatch = await Task.WhenAll(carddefAndIsMatchTasks);

                return carddefAndIsMatch
                    .Where(x => x.IsMatch)
                    .Select(x => x.Carddef);
            }

            var carddefsTasks = _this.OutZoneCondition.Value
                .Select(async zoneType => zoneType switch
                {
                    OutZonePrettyName.CardPool => await GetMatchedFromPool(),
                    OutZonePrettyName.YouExcluded =>
                        effectEventArgs.GameMaster.Get(effectOwnerCard.OwnerId)?.Excludes ?? Enumerable.Empty<CardDef>(),
                    OutZonePrettyName.OpponentExcluded =>
                        effectEventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Excludes,
                    OutZonePrettyName.OwnerExcluded =>
                        effectEventArgs.GameMaster.Get(effectOwnerCard.OwnerId)?.Excludes ?? Enumerable.Empty<CardDef>(),
                    _ => Array.Empty<CardDef>(),
                });

            var carddefs = await Task.WhenAll(carddefsTasks);

            return carddefs
                .SelectMany(c => c)
                .ToArray();
        }

        public static async ValueTask<bool> IsMatch(this CardDefCondition _this,
            CardDef cardDefToMatch, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return
                (_this.CostCondition?.IsMatch(cardDefToMatch.Cost) ?? true)
                && (_this.PowerCondition?.IsMatch(cardDefToMatch.Power) ?? true)
                && (_this.ToughnessCondition?.IsMatch(cardDefToMatch.Toughness) ?? true)
                && (await (_this.CardSetCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardDefToMatch) ?? ValueTask.FromResult(true)))
                && (await (_this.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardDefToMatch.Name) ?? ValueTask.FromResult(true)))
                && (_this.TypeCondition?.IsMatch(cardDefToMatch.Type) ?? true);
        }
    }
}
