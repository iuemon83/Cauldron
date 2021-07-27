using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionDamage
    {
        public NumValue Value { get; }
        public Choice Choice { get; }

        public EffectActionDamage(NumValue value, Choice choice)
        {
            this.Value = value;
            this.Choice = choice;
        }
    }
}