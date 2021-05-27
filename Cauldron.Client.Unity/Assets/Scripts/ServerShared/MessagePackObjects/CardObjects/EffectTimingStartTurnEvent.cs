using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingStartTurnEvent
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

        public EffectTimingStartTurnEvent.EventSource Source { get; }

        public EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource Source)
        {
            this.Source = Source;
        }
    }
}
