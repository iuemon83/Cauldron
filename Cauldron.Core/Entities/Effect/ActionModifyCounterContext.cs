using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionModifyCounterContext(
        IReadOnlyList<Card> ModifiedCards,
        int NumBeforeCounters,
        int NumAfterCounters
        )
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfModifyCounter.TypeValue type)
            => type switch
            {
                ActionContextCardsOfModifyCounter.TypeValue.Modified => this.ModifiedCards,
                _ => Array.Empty<Card>()
            };

        public int GetCounters(ActionContextCountersOfModifyCounter.TypeValue type)
            => type switch
            {
                ActionContextCountersOfModifyCounter.TypeValue.Before => this.NumBeforeCounters,
                ActionContextCountersOfModifyCounter.TypeValue.After => this.NumAfterCounters,
                ActionContextCountersOfModifyCounter.TypeValue.Modified => Math.Abs(this.NumAfterCounters - this.NumBeforeCounters),
                _ => 0
            };
    }
}
