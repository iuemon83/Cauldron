using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingEndTurnEvent
    {
        public enum EventSource
        {
            You,
            Opponent,
            Both
        }

        public EffectTimingEndTurnEvent.EventSource Source { get; }

        public EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource Source)
        {
            this.Source = Source;
        }
    }
}
