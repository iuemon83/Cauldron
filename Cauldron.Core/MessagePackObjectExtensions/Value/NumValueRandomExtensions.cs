using Cauldron.Core;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class NumValueRandomExtensions
    {
        public static int Calculate(this NumValueRandom _this)
        {
            return RandomUtil.RandomValue(_this.Min, _this.Max);
        }
    }
}
