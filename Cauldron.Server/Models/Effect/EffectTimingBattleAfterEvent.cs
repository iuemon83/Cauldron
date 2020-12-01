namespace Cauldron.Server.Models.Effect
{
    public record EffectTimingBattleAfterEvent(
        EffectTimingBattleBeforeEvent.EventSource Source,
        PlayerCondition PlayerCondition,
        CardCondition CardCondition) : EffectTimingBattleBeforeEvent(Source, PlayerCondition, CardCondition);
}
