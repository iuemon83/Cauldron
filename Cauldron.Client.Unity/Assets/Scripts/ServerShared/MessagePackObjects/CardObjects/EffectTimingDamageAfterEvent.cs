#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDamageAfterEvent : EffectTimingDamageBeforeEvent
    {
        public EffectTimingDamageAfterEvent(
            EffectTimingDamageBeforeEvent.TypeValue Type = TypeValue.Any,
            CardCondition? SourceCardCondition = null,
            PlayerCondition? TakePlayerCondition = null,
            CardCondition? TakeCardCondition = null
            )
            : base(Type, SourceCardCondition, TakePlayerCondition, TakeCardCondition)
        {
        }
    }
}
