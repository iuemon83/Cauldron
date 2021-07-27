using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingStartTurnEvent
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

        public EffectTimingStartTurnEvent.SourceValue Source { get; }

        public EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.SourceValue Source)
        {
            this.Source = Source;
        }
    }
}
