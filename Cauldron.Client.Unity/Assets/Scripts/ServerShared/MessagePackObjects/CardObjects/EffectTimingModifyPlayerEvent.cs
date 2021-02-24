using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingModifyPlayerEvent
    {
        public enum EventSource
        {
            Owner,
            Other,
            All
        }

        public EffectTimingModifyPlayerEvent.EventSource Source { get; }
        public bool ModifyMaxHp { get; }
        public bool ModifyHp { get; }
        public bool ModifyMaxMp { get; }
        public bool ModifyMp { get; }

        public EffectTimingModifyPlayerEvent(
            EffectTimingModifyPlayerEvent.EventSource Source,
            bool ModifyMaxHp,
            bool ModifyHp,
            bool ModifyMaxMp,
            bool ModifyMp
            )
        {
            this.Source = Source;
            this.ModifyMaxHp = ModifyMaxHp;
            this.ModifyHp = ModifyHp;
            this.ModifyMaxMp = ModifyMaxMp;
            this.ModifyMp = ModifyMp;
        }
    }
}
