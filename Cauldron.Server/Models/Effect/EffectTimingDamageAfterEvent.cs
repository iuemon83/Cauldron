namespace Cauldron.Server.Models.Effect
{
    public record EffectTimingDamageAfterEvent(
        EffectTimingDamageBeforeEvent.EventSource Source,
        PlayerCondition PlayerCondition,
        CardCondition CardCondition) : EffectTimingDamageBeforeEvent(Source, PlayerCondition, CardCondition);
}
