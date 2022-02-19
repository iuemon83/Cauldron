#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingAttackAfterEvent : EffectTimingAttackBeforeEvent
    {
        public EffectTimingAttackAfterEvent(
            CardCondition? AttackCardCondition = null,
            CardCondition? GuardCardCondition = null
            )
            : base(AttackCardCondition, GuardCardCondition) { }
    }
}
