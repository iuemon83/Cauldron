using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDamageAfterEvent : EffectTimingDamageBeforeEvent
    {
        public EffectTimingDamageAfterEvent(
            EffectTimingDamageBeforeEvent.DamageType Type = DamageType.Any,
            EffectTimingDamageBeforeEvent.EventSource Source = EventSource.Any,
            PlayerCondition PlayerCondition = null,
            CardCondition CardCondition = null
            )
            : base(Type, Source, PlayerCondition, CardCondition)
        {
        }
    }
}
