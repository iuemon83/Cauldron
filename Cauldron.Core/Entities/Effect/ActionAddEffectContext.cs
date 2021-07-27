using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Cauldron.Core.Entities.Effect
{
    public record ActionAddEffectContext(IReadOnlyList<Card> TargetdCards)
    {
        public IEnumerable<Card> GetCards(ActionContextCardsOfAddEffect.TypeValue type)
            => type switch
            {
                ActionContextCardsOfAddEffect.TypeValue.TargetCards => this.TargetdCards,
                _ => Array.Empty<Card>()
            };

        public int GetNum() => 0;

        public string GetText() => "";
    }
}
