namespace Cauldron.Server.Models.Effect
{
    public record EffectWhen(
        EffectTiming Timing
        )
    {
        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
            => this.Timing.IsMatch(effectOwnerCard, eventArgs);
    }
}
