using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionDamage
    {
        public NumValue Value { get; }
        public Choice Choice { get; }
        public string Name { get; }

        public EffectActionDamage(NumValue value, Choice choice,
            string Name = null)
        {
            this.Value = value;
            this.Choice = choice;
            this.Name = Name;
        }
    }
}
