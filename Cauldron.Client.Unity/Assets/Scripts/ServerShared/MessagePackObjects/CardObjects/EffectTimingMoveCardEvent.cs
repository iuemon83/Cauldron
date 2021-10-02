using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カードの移動時
    /// </summary>
    [MessagePackObject(true)]
    public class EffectTimingMoveCardEvent
    {
        public CardCondition[] OrCardConditions { get; }

        public ZonePrettyName From { get; }
        public ZonePrettyName To { get; }

        public EffectTimingMoveCardEvent(
            CardCondition[] OrCardConditions,
            ZonePrettyName From = ZonePrettyName.None,
            ZonePrettyName To = ZonePrettyName.None
            )
        {
            this.OrCardConditions = OrCardConditions;
            this.From = From;
            this.To = To;
        }
    }
}
