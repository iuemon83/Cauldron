using Cauldron.Server.Models.Effect.Value;

namespace Cauldron.Server.Models.Effect
{
    public record EffectIf(
        NumCondition NumCondition,
        NumValue NumValue
        )
    {
        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var value = this.NumValue.Calculate(effectOwnerCard, eventArgs);
            return this.NumCondition.IsMatch(value);
        }
    }
}
