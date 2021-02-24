using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDamageBeforeEvent
    {
        public enum EventSource
        {
            All,
            DamageSource,
            Guard,
        }

        public EffectTimingDamageBeforeEvent.EventSource Source { get; }
        public PlayerCondition PlayerCondition { get; }
        public CardCondition CardCondition { get; }

        public EffectTimingDamageBeforeEvent(
            EffectTimingDamageBeforeEvent.EventSource Source,
            PlayerCondition PlayerCondition = null,
            CardCondition CardCondition = null
            )
        {
            this.Source = Source;
            this.PlayerCondition = PlayerCondition;
            this.CardCondition = CardCondition;
        }
    }
}
