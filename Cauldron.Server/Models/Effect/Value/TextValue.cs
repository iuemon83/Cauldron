namespace Cauldron.Server.Models.Effect.Value
{
    public record TextValue(
        string PureValue = null,
        TextValueCalculator TextValueCalculator = null
        )
    {
        public string Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return this.PureValue
                ?? this.TextValueCalculator?.Calculate(effectOwnerCard, effectEventArgs)
                ?? "";
        }
    }
}
