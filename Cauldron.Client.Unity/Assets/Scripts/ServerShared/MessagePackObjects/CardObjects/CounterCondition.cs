using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CounterCondition
    {
        public string CounterName { get; }

        public NumCompare NumCountersCondition { get; }

        public CounterCondition(string CounterName, NumCompare NumCountersCondition)
        {
            this.CounterName = CounterName;
            this.NumCountersCondition = NumCountersCondition;
        }
    }
}
