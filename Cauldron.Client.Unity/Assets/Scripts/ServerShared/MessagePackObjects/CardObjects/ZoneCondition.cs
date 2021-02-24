using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ZoneCondition
    {
        public ZoneValue Value { get; set; }
        public bool Not { get; set; }

        public ZoneCondition(ZoneValue value, bool not = false)
        {
            this.Value = value;
            this.Not = not;
        }
    }
}
