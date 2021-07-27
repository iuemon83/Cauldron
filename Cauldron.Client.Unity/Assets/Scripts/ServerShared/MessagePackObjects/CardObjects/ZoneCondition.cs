using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ZoneCondition
    {
        public ZoneValue Value { get; }
        public bool Not { get; }

        public ZoneCondition(ZoneValue value, bool not = false)
        {
            this.Value = value;
            this.Not = not;
        }
    }
}
