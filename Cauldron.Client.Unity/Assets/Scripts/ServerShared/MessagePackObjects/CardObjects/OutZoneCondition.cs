using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class OutZoneCondition
    {
        public OutZonePrettyName[] Value { get; }
        public bool Not { get; }

        public OutZoneCondition(OutZonePrettyName[] value, bool not = false)
        {
            this.Value = value ?? Array.Empty<OutZonePrettyName>();
            this.Not = not;
        }
    }
}
