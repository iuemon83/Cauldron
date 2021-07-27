using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDestroyEvent
    {
        public enum SourceValue
        {
            [DisplayText("このカード")]
            This,
            [DisplayText("その他のカード")]
            Other
        }

        public EffectTimingDestroyEvent.SourceValue Source { get; }

        public EffectTimingDestroyEvent(EffectTimingDestroyEvent.SourceValue Source)
        {
            this.Source = Source;
        }
    }
}
