using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDestroyEvent
    {
        public enum EventSource
        {
            This,
            Other
        }

        public EffectTimingDestroyEvent.EventSource Source { get; }

        public EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource Source)
        {
            this.Source = Source;
        }
    }
}
