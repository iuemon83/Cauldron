#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ActionContextCounters
    {
        public ActionContextCountersOfModifyCounter? OfModifyCounter { get; }

        public ActionContextCounters(
            ActionContextCountersOfModifyCounter? OfModifyCounter = null
            )
        {
            this.OfModifyCounter = OfModifyCounter;
        }
    }
}
