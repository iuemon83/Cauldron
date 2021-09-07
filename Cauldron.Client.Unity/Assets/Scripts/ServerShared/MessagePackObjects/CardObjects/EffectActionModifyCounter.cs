using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionModifyCounter
    {
        public Choice TargetsChoice { get; }

        public string CounterName { get; }

        public NumValueModifier NumCountersModifier { get; }

        public string Name { get; }

        public EffectActionModifyCounter(Choice TargetsChoice, string CounterName,
            NumValueModifier NumCountersModifier, string Name = null)
        {
            this.TargetsChoice = TargetsChoice;
            this.CounterName = CounterName;
            this.NumCountersModifier = NumCountersModifier;
            this.Name = Name;
        }
    }
}
