using Cauldron.Core.Entities;
using Cauldron.Core.Entities.Effect;


namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardDefConditionExtensions
    {
        public static async ValueTask<CardDef[]> ListMatchedCardDefs(this CardDefCondition _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs, CardRepository cardRepository)
        {
            async ValueTask<IEnumerable<CardDef>> GetMatchedList(IEnumerable<CardDef>? list)
            {
                if (list == null)
                {
                    return Enumerable.Empty<CardDef>();
                }

                var carddefAndIsMatchTasks = list
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
                    OutZonePrettyName.CardPool => await GetMatchedList(cardRepository.CardPool),
                    OutZonePrettyName.YouExcluded => await GetMatchedList(
                        effectEventArgs.GameMaster.Get(effectOwnerCard.OwnerId)?.Excludes),
                    OutZonePrettyName.OpponentExcluded => await GetMatchedList(
                        effectEventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Excludes),
                    OutZonePrettyName.OwnerExcluded => await GetMatchedList(
                        effectEventArgs.GameMaster.Get(effectOwnerCard.OwnerId)?.Excludes),
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
                (await (_this.CostCondition?.IsMatch(cardDefToMatch.Cost, effectOwnerCard, effectEventArgs)
                    ?? ValueTask.FromResult(true)))
                && (await (_this.PowerCondition?.IsMatch(cardDefToMatch.Power, effectOwnerCard, effectEventArgs)
                    ?? ValueTask.FromResult(true)))
                && (await (_this.ToughnessCondition?.IsMatch(cardDefToMatch.Toughness, effectOwnerCard, effectEventArgs)
                    ?? ValueTask.FromResult(true)))
                && (await (_this.CardSetCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardDefToMatch)
                    ?? ValueTask.FromResult(true)))
                && (await (_this.NameCondition?.IsMatch(effectOwnerCard, effectEventArgs, cardDefToMatch.Name)
                    ?? ValueTask.FromResult(true)))
                && (_this.TypeCondition?.IsMatch(cardDefToMatch.Type) ?? true);
        }
    }
}
