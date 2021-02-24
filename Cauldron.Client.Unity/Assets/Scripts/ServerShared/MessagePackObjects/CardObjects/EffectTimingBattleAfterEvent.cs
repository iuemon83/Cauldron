using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingBattleAfterEvent : EffectTimingBattleBeforeEvent
    {
        public EffectTimingBattleAfterEvent(
            EffectTimingBattleBeforeEvent.EventSource Source,
            PlayerCondition PlayerCondition = null,
            CardCondition CardCondition = null
            )
            : base(Source, PlayerCondition, CardCondition)
        { }
    }
}
