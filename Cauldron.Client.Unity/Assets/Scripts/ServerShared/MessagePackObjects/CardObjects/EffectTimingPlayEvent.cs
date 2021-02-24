using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingPlayEvent
    {
        public enum EventSource
        {
            This,
            Other
        }

        public EffectTimingPlayEvent.EventSource Source { get; }

        public EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource Source)
        {
            this.Source = Source;
        }
    }
}
