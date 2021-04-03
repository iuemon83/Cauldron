using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueVariableCalculatorExtensions
    {
        public static int Calculate(this NumValueVariableCalculator numValueVariableCalculator, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return effectEventArgs.GameMaster.TryGetNumVariable(effectOwnerCard.Id, numValueVariableCalculator.Name, out var value)
                ? value
                : default;
        }
    }
}
