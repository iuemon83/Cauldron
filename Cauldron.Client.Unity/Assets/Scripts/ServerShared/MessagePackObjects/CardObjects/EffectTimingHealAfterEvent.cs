#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingHealAfterEvent : EffectTimingHealBeforeEvent
    {
        public EffectTimingHealAfterEvent(
            CardCondition? SourceCardCondition = null,
            PlayerCondition? TakePlayerCondition = null,
            CardCondition? TakeCardCondition = null
            )
            : base(SourceCardCondition, TakePlayerCondition, TakeCardCondition)
        {
        }
    }
}
