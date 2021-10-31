﻿using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectIf
    {
        public ConditionWrap Condition { get; }

        public EffectIf(ConditionWrap Condition)
        {
            this.Condition = Condition;
        }
    }
}
