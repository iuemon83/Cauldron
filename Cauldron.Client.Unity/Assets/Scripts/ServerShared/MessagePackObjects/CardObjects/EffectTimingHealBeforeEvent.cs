#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingHealBeforeEvent
    {
        public CardCondition? SourceCardCondition { get; }
        public PlayerCondition? TakePlayerCondition { get; }
        public CardCondition? TakeCardCondition { get; }

        public EffectTimingHealBeforeEvent(
            CardCondition? SourceCardCondition = null,
            PlayerCondition? TakePlayerCondition = null,
            CardCondition? TakeCardCondition = null
            )
        {
            this.TakePlayerCondition = TakePlayerCondition;
            this.SourceCardCondition = SourceCardCondition;
            this.TakeCardCondition = TakeCardCondition;
        }
    }
}
