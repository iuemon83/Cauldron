using MessagePack;
using System;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardEffect
    {
        public EffectConditionWrap Condition { get; }

        public IReadOnlyList<EffectAction> Actions { get; }

        public CardEffect(
            EffectConditionWrap Condition,
            IReadOnlyList<EffectAction> Actions
            )
        {
            this.Condition = Condition;
            this.Actions = Actions ?? Array.Empty<EffectAction>();
        }
    }
}
