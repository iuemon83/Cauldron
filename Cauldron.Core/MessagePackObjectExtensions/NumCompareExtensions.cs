using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class NumCompareExtensions
    {
        public static bool IsMatch(this NumCompare _this, int checkValue)
        {
            var result = _this.Compare switch
            {
                NumCompare.CompareValue.Equality => checkValue == _this.Value,
                NumCompare.CompareValue.LessThan => checkValue <= _this.Value,
                NumCompare.CompareValue.GreaterThan => checkValue >= _this.Value,
                _ => throw new InvalidOperationException($"{nameof(_this.Compare)}: {_this.Compare}")
            };

            return result;
        }
    }
}
