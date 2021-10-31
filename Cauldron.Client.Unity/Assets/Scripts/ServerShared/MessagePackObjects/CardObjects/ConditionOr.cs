using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ConditionOr
    {
        public ConditionWrap[] Conditions { get; }

        public ConditionOr(ConditionWrap[] Conditions)
        {
            this.Conditions = Conditions ?? Array.Empty<ConditionWrap>();
        }
    }
}
