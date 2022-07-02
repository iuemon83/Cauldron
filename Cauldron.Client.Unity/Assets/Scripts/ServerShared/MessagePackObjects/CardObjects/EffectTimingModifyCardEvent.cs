#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingModifyCardEvent
    {
        public CardCondition[] OrCardConditions { get; }
        public NumCompare? ModifyPowerCondition { get; }
        public NumCompare? ModifyToughnessCondition { get; }

        public EffectTimingModifyCardEvent(
            CardCondition[] OrCardConditions,
            NumCompare? ModifyPowerCondition = default,
            NumCompare? ModifyToughnessCondition = default
            )
        {
            this.OrCardConditions = OrCardConditions;
            this.ModifyPowerCondition = ModifyPowerCondition;
            this.ModifyToughnessCondition = ModifyToughnessCondition;
        }
    }
}
