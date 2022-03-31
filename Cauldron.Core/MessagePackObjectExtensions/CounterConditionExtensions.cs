using Cauldron.Core.Entities.Effect;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class CounterConditionExtensions
    {
        public static async ValueTask<bool> IsMatch(this CounterCondition _this,
            Card cardToMatch, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var counter = cardToMatch.GetCounter(_this.CounterName);
            return await _this.IsMatch(_this.CounterName, counter, effectOwnerCard, effectEventArgs);
        }

        public static async ValueTask<bool> IsMatch(this CounterCondition _this,
            string counterName, int numCounters, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return _this.CounterName == counterName
                && (await _this.NumCountersCondition.IsMatch(numCounters, effectOwnerCard, effectEventArgs));
        }
    }
}
