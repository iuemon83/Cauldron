using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingBattleBeforeEvent
    {
        public enum EventSource
        {
            All,
            Attack,
            Guard,
        }

        public EffectTimingBattleBeforeEvent.EventSource Source { get; }
        public PlayerCondition PlayerCondition { get; }
        public CardCondition CardCondition { get; }

        public EffectTimingBattleBeforeEvent(
            EffectTimingBattleBeforeEvent.EventSource Source,
            PlayerCondition PlayerCondition = null,
            CardCondition CardCondition = null
            )
        {
            this.Source = Source;
            this.PlayerCondition = PlayerCondition;
            this.CardCondition = CardCondition;
        }
    }
}
