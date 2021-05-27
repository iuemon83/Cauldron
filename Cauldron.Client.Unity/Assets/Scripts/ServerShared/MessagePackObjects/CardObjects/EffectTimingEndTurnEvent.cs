using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingEndTurnEvent
    {
        public enum EventSource
        {
            [DisplayText("あなた")]
            You,
            [DisplayText("相手")]
            Opponent,
            [DisplayText("両方")]
            Both
        }

        public EffectTimingEndTurnEvent.EventSource Source { get; }

        public EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource Source)
        {
            this.Source = Source;
        }
    }
}
