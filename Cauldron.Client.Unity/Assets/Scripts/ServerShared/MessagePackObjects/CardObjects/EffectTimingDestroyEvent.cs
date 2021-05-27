using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDestroyEvent
    {
        public enum EventSource
        {
            [DisplayText("このカード")]
            This,
            [DisplayText("その他のカード")]
            Other
        }

        public EffectTimingDestroyEvent.EventSource Source { get; }

        public EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource Source)
        {
            this.Source = Source;
        }
    }
}
