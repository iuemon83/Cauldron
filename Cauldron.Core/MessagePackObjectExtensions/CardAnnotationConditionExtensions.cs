using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class CardAnnotationConditionExtensions
    {
        public static bool IsMatch(this CardAnnotationCondition _this, List<string> values)
        {
            var result = values?.Contains(_this.Value) ?? false;
            return _this.Not ? !result : result;
        }
    }
}
