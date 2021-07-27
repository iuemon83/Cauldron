using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingEndTurnEvent
    {
        public enum SourceValue
        {
            [DisplayText("あなた")]
            You,
            [DisplayText("相手")]
            Opponent,
            [DisplayText("両方")]
            Both
        }

        public EffectTimingEndTurnEvent.SourceValue Source { get; }

        public EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.SourceValue Source)
        {
            this.Source = Source;
        }
    }
}
