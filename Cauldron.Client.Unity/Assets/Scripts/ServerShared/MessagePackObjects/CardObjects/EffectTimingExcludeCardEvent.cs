#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カードの除外時
    /// </summary>
    [MessagePackObject(true)]
    public class EffectTimingExcludeCardEvent
    {
        public CardCondition[] OrCardConditions { get; }
        public ZoneCondition? FromZoneCondition { get; }

        public EffectTimingExcludeCardEvent(
            CardCondition[] OrCardConditions,
            ZoneCondition? FromZoneCondition = null
            )
        {
            this.OrCardConditions = OrCardConditions;
            this.FromZoneCondition = FromZoneCondition;
        }
    }
}
