using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class NumCompareExtensions
    {
        public static async ValueTask<bool> IsMatch(this NumCompare _this, int checkValue, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var value = await _this.Value.Calculate(effectOwnerCard, effectEventArgs);

            var result = _this.Compare switch
            {
                NumCompare.CompareValue.Equality => checkValue == value,
                NumCompare.CompareValue.LessThan => checkValue <= value,
                NumCompare.CompareValue.GreaterThan => checkValue >= value,
                _ => throw new InvalidOperationException($"{nameof(_this.Compare)}: {_this.Compare}")
            };

            return result;
        }
    }
}
