using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class TextConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this TextCondition textCondition, Card effectOwnerCard, EffectEventArgs effectEventArgs, string checkValue)
        {
            var value = await textCondition.Value.Calculate(effectOwnerCard, effectEventArgs);

            var result = textCondition.Compare switch
            {
                TextCondition.CompareValue.Equality => checkValue == value,
                TextCondition.CompareValue.Contains => checkValue.Contains(value),
                _ => throw new InvalidOperationException($"不正な入力値です: {textCondition.Compare}")
            };

            return textCondition.Not ? !result : result;
        }
    }
}
