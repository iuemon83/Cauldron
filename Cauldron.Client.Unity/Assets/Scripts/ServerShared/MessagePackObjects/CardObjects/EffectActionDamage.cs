using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionDamage
    {
        public int Value { get; set; }
        public Choice Choice { get; set; }

        public EffectActionDamage(int value, Choice choice)
        {
            this.Value = value;
            this.Choice = choice;
        }
    }
}