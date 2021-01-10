namespace Cauldron.Server.Models.Effect.Value
{
    public record NumValue(
        int? PureValue = null,
        NumValueCalculator NumValueCalculator = null,
        NumValueVariableCalculator NumValueVariableCalculator = null,
        NumValueModifier NumValueModifier = null
        )
    {
        public int Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var baseValue = this.PureValue
                ?? this.NumValueCalculator?.Calculate(effectOwnerCard, effectEventArgs)
                ?? this.NumValueVariableCalculator?.Calculate(effectOwnerCard, effectEventArgs)
                ?? 0;

            return this.NumValueModifier?.Modify(effectOwnerCard, effectEventArgs, baseValue)
                ?? baseValue;
        }
    }
}
