using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カードの移動時
    /// </summary>
    [MessagePackObject(true)]
    public class EffectTimingMoveCardEvent
    {
        public enum SourceValue
        {
            [DisplayText("このカード")]
            This,
            [DisplayText("他のカード")]
            Other
        }

        public EffectTimingMoveCardEvent.SourceValue Source { get; }
        public ZonePrettyName From { get; }
        public ZonePrettyName To { get; }

        public EffectTimingMoveCardEvent(
            EffectTimingMoveCardEvent.SourceValue Source,
            ZonePrettyName From,
            ZonePrettyName To
            )
        {
            this.Source = Source;
            this.From = From;
            this.To = To;
        }
    }
}
