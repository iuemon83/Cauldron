using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingModifyPlayerEvent
    {
        public enum SourceValue
        {
            [DisplayText("あなた")]
            Owner,
            [DisplayText("その他")]
            Other,
            [DisplayText("すべて")]
            All
        }

        public EffectTimingModifyPlayerEvent.SourceValue Source { get; }
        public bool ModifyMaxHp { get; }
        public bool ModifyHp { get; }
        public bool ModifyMaxMp { get; }
        public bool ModifyMp { get; }

        public EffectTimingModifyPlayerEvent(
            EffectTimingModifyPlayerEvent.SourceValue Source,
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
