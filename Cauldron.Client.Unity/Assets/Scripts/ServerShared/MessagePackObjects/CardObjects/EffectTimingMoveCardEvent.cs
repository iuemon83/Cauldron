#nullable enable

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

        public ZoneCondition? From { get; }
        public ZoneCondition? To { get; }

        public EffectTimingMoveCardEvent(
            CardCondition[] OrCardConditions,
            ZoneCondition? From = null,
            ZoneCondition? To = null
            )
        {
            this.OrCardConditions = OrCardConditions;
            this.From = From;
            this.To = To;
        }
    }
}
