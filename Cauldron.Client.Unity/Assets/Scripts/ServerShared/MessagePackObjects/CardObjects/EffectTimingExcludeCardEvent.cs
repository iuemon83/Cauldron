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

        public EffectTimingExcludeCardEvent(CardCondition[] OrCardConditions)
        {
            this.OrCardConditions = OrCardConditions;
        }
    }
}
