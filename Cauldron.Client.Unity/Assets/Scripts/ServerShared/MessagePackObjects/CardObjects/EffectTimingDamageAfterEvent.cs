#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDamageAfterEvent : EffectTimingDamageBeforeEvent
    {
        public EffectTimingDamageAfterEvent(
            EffectTimingDamageBeforeEvent.TypeValue Type = TypeValue.Any,
            EffectTimingDamageBeforeEvent.SourceValue Source = SourceValue.Any,
            PlayerCondition? PlayerCondition = null,
            CardCondition? CardCondition = null
            )
            : base(Type, Source, PlayerCondition, CardCondition)
        {
        }
    }
}
