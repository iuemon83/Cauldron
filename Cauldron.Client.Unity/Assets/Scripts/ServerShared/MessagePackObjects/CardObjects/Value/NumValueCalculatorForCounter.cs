using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculatorForCounter
    {
        public string CounterName { get; }

        public Choice TargetChoice { get; }

        public ActionContextCounters ActionContextCounters { get; }

        public NumValueCalculatorForCounter(
            string CounterName = null,
            Choice TargetChoice = null,
            ActionContextCounters ActionContextCounters = null
            )
        {
            this.CounterName = CounterName;
            this.TargetChoice = TargetChoice;
            this.ActionContextCounters = ActionContextCounters;
        }
    }
}
