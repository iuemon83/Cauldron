using MessagePack;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardEffect
    {
        public EffectCondition Condition { get; }

        public IReadOnlyList<EffectAction> Actions { get; }

        public CardEffect(EffectCondition condition, IReadOnlyList<EffectAction> actions)
        {
            this.Condition = condition;
            this.Actions = actions;
        }
    }
}
