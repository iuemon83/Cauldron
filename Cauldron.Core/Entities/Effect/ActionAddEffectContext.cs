using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionAddEffectContext(IReadOnlyList<Card> TargetdCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfAddEffect.ValueType type)
            => type switch
            {
                ActionContextCardsOfAddEffect.ValueType.TargetCards => this.TargetdCards,
                _ => Array.Empty<Card>()
            };

        public int GetNum() => 0;

        public string GetText() => "";
    }
}
