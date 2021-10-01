using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingModifyPlayerEvent
    {
        public PlayerCondition[] OrPlayerCondition { get; }

        public bool ModifyMaxHp { get; }
        public bool ModifyHp { get; }
        public bool ModifyMaxMp { get; }
        public bool ModifyMp { get; }

        public EffectTimingModifyPlayerEvent(
            PlayerCondition[] OrPlayerCondition,
            bool ModifyMaxHp,
            bool ModifyHp,
            bool ModifyMaxMp,
            bool ModifyMp
            )
        {
            this.OrPlayerCondition = OrPlayerCondition;
            this.ModifyMaxHp = ModifyMaxHp;
            this.ModifyHp = ModifyHp;
            this.ModifyMaxMp = ModifyMaxMp;
            this.ModifyMp = ModifyMp;
        }
    }
}
