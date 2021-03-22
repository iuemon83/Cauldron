using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionDamage
    {
        public NumValue Value { get; set; }
        public Choice Choice { get; set; }

        public EffectActionDamage(NumValue value, Choice choice)
        {
            this.Value = value;
            this.Choice = choice;
        }
    }
}