using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カードの移動時
    /// </summary>
    [MessagePackObject(true)]
    public class EffectTimingMoveCardEvent
    {
        public enum EventSource
        {
            This,
            Other
        }

        public EffectTimingMoveCardEvent.EventSource Source { get; }
        public ZonePrettyName From { get; }
        public ZonePrettyName To { get; }

        public EffectTimingMoveCardEvent(
            EffectTimingMoveCardEvent.EventSource Source,
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
