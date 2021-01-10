namespace Cauldron.Server.Models.Effect
{
    public record EffectIf(
        NumCondition NumCondition,
        ValueCalculator ValueCalculator
        )
    {
        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var value = this.ValueCalculator.Calculate(effectOwnerCard, eventArgs);
            return this.NumCondition.IsMatch(value);
        }
    }
}
