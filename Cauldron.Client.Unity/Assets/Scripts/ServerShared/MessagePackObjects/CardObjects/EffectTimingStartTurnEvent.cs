using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingStartTurnEvent
    {
        public enum EventSource
        {
            You,
            Opponent,
            Both
        }

        public EffectTimingStartTurnEvent.EventSource Source { get; }

        public EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource Source)
        {
            this.Source = Source;
        }
    }
}
