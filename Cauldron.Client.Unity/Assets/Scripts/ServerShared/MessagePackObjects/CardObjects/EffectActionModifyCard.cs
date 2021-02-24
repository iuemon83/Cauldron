using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionModifyCard
    {
        public NumValue Power { get; set; }
        public NumValue Toughness { get; set; }
        public Choice Choice { get; set; }

        public EffectActionModifyCard(
            NumValue Power,
            NumValue Toughness,
            Choice Choice
            )
        {
            this.Power = Power;
            this.Toughness = Toughness;
            this.Choice = Choice;
        }
    }
}
