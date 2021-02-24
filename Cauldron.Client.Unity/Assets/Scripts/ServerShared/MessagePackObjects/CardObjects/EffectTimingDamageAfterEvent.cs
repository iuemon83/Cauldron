using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDamageAfterEvent : EffectTimingDamageBeforeEvent
    {
        public EffectTimingDamageAfterEvent(
            EffectTimingDamageBeforeEvent.EventSource Source,
            PlayerCondition PlayerCondition = null,
            CardCondition CardCondition = null
            )
            : base(Source, PlayerCondition, CardCondition)
        {
        }
    }
}
