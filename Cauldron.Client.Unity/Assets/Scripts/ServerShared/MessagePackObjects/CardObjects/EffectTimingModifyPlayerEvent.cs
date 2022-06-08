#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingModifyPlayerEvent
    {
        public PlayerCondition[] OrPlayerConditions { get; }

        public NumCompare? ModifyMaxHpCondition { get; }
        public NumCompare? ModifyCurrentHpCondition { get; }
        public NumCompare? ModifyMaxMpCondition { get; }
        public NumCompare? ModifyCurrentMpCondition { get; }

        public EffectTimingModifyPlayerEvent(
            PlayerCondition[] OrPlayerConditions,
            NumCompare? ModifyMaxHpCondition = default,
            NumCompare? ModifyCurrentHpCondition = default,
            NumCompare? ModifyMaxMpCondition = default,
            NumCompare? ModifyCurrentMpCondition = default
            )
        {
            this.OrPlayerConditions = OrPlayerConditions;
            this.ModifyMaxHpCondition = ModifyMaxHpCondition;
            this.ModifyCurrentHpCondition = ModifyCurrentHpCondition;
            this.ModifyMaxMpCondition = ModifyMaxMpCondition;
            this.ModifyCurrentMpCondition = ModifyCurrentMpCondition;
        }
    }
}
