using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ConditionAnd
    {
        public ConditionWrap[] Conditions { get; }

        public ConditionAnd(ConditionWrap[] Conditions)
        {
            this.Conditions = Conditions ?? Array.Empty<ConditionWrap>();
        }
    }
}
