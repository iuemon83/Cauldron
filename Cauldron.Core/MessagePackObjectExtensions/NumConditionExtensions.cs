using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class NumConditionExtensions
    {
        public static bool IsMatch(this NumCondition numCondition, int checkValue)
        {
            var result = numCondition.Compare switch
            {
                NumCondition.CompareValue.Equality => checkValue == numCondition.Value,
                NumCondition.CompareValue.LessThan => checkValue <= numCondition.Value,
                NumCondition.CompareValue.GreaterThan => checkValue >= numCondition.Value,
                _ => throw new InvalidOperationException($"{nameof(numCondition.Compare)}: {numCondition.Compare}")
            };

            return result ^ numCondition.Not;
        }
    }
}
