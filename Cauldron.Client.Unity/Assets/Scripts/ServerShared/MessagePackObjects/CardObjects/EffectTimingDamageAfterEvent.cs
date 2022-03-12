#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDamageAfterEvent : EffectTimingDamageBeforeEvent
    {
        public EffectTimingDamageAfterEvent(
            EffectTimingDamageBeforeEvent.TypeValue Type = TypeValue.Any,
            PlayerCondition? TakePlayerCondition = null,
            CardCondition? SourceCardCondition = null,
            CardCondition? TakeCardCondition = null
            )
            : base(Type, TakePlayerCondition, SourceCardCondition, TakeCardCondition)
        {
        }
    }
}
