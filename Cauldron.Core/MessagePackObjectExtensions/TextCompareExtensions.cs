using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class TextCompareExtensions
    {
        public static async ValueTask<bool> IsMatch(this TextCompare _this,
            Card effectOwnerCard, EffectEventArgs effectEventArgs, string checkValue)
        {
            var value = await _this.Value.Calculate(effectOwnerCard, effectEventArgs);

            var result = _this.Compare switch
            {
                TextCompare.CompareValue.Equality => checkValue == value,
                TextCompare.CompareValue.Contains => checkValue.Contains(value),
                _ => throw new InvalidOperationException($"不正な入力値です: {_this.Compare}")
            };

            return result ^ _this.Not;
        }
    }
}
