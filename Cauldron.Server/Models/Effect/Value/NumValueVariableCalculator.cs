namespace Cauldron.Server.Models.Effect.Value
{
    public record NumValueVariableCalculator(string Name)
    {
        public int Calculate(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return effectEventArgs.GameMaster.TryGetNumVariable(effectOwnerCard.Id, this.Name, out var value)
                ? value
                : default;
        }
    }
}
