using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingModifyPlayerEvent
    {
        public PlayerCondition[] OrPlayerConditions { get; }

        public bool ModifyMaxHp { get; }
        public bool ModifyHp { get; }
        public bool ModifyMaxMp { get; }
        public bool ModifyMp { get; }

        public EffectTimingModifyPlayerEvent(
            PlayerCondition[] OrPlayerConditions,
            bool ModifyMaxHp,
            bool ModifyHp,
            bool ModifyMaxMp,
            bool ModifyMp
            )
        {
            this.OrPlayerConditions = OrPlayerConditions;
            this.ModifyMaxHp = ModifyMaxHp;
            this.ModifyHp = ModifyHp;
            this.ModifyMaxMp = ModifyMaxMp;
            this.ModifyMp = ModifyMp;
        }
    }
}
