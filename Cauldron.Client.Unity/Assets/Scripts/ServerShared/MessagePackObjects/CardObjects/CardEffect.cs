using MessagePack;
using System;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardEffect
    {
        public string Description { get; }

        public EffectConditionWrap Condition { get; }

        public IReadOnlyList<EffectAction> Actions { get; }

        public CardEffect(
            string description,
            EffectConditionWrap Condition,
            IReadOnlyList<EffectAction> Actions
            )
        {
            this.Description = description;
            this.Condition = Condition;
            this.Actions = Actions ?? Array.Empty<EffectAction>();
        }
    }
}
