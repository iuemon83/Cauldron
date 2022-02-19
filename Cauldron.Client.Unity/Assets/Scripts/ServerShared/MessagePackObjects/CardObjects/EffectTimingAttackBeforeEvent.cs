#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingAttackBeforeEvent
    {
        public CardCondition? AttackCardCondition { get; }

        public CardCondition? GuardCardCondition { get; }

        public EffectTimingAttackBeforeEvent(
            CardCondition? AttackCardCondition = null,
            CardCondition? GuardCardCondition = null
            )
        {
            this.AttackCardCondition = AttackCardCondition;
            this.GuardCardCondition = GuardCardCondition;
        }
    }
}
