namespace Cauldron.Server.Models.Effect
{
    public record ValueCalculator(
        ValueCalculatorForCard Card)
    {
        public int Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return this.Card.Calculate(effectOwnerCard, effectEventArgs);
        }
    }
}
