namespace Cauldron.Shared.MessagePackObjects
{
    public static class CounterConditionExtensions
    {
        public static bool IsMatch(this CounterCondition _this, Card cardToMatch)
        {
            var counter = cardToMatch.GetCounter(_this.CounterName);
            return _this.IsMatch(_this.CounterName, counter);
        }

        public static bool IsMatch(this CounterCondition _this, string counterName, int numCounters)
        {
            return _this.CounterName == counterName
                && _this.NumCountersCondition.IsMatch(numCounters);
        }
    }
}
