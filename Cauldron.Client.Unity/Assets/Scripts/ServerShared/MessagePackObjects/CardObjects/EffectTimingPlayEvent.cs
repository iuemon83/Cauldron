using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingPlayEvent
    {
        public enum EventSource
        {
            [DisplayText("このカード")]
            This,
            [DisplayText("他のカード")]
            Other
        }

        public EffectTimingPlayEvent.EventSource Source { get; }

        public EffectTimingPlayEvent(EffectTimingPlayEvent.EventSource Source)
        {
            this.Source = Source;
        }
    }
}
