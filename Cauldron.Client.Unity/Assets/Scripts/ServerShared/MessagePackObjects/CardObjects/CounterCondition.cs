using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CounterCondition
    {
        public string CounterName { get; }

        public NumCondition NumCountersCondition { get; }

        public CounterCondition(string CounterName, NumCondition NumCountersCondition)
        {
            this.CounterName = CounterName;
            this.NumCountersCondition = NumCountersCondition;
        }
    }
}
