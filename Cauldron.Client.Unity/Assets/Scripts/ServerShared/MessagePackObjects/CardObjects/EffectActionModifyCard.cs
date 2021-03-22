using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionModifyCard
    {
        public NumValueModifier Cost { get; set; }
        public NumValueModifier Power { get; set; }
        public NumValueModifier Toughness { get; set; }
        public Choice Choice { get; set; }

        public EffectActionModifyCard(
            Choice Choice,
            NumValueModifier Cost = null,
            NumValueModifier Power = null,
            NumValueModifier Toughness = null
            )
        {
            this.Cost = Cost;
            this.Power = Power;
            this.Toughness = Toughness;
            this.Choice = Choice;
        }
    }
}
