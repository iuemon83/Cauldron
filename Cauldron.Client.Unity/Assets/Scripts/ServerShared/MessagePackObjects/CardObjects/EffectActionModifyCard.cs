using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionModifyCard
    {
        public NumValueModifier Cost { get; }
        public NumValueModifier Power { get; }
        public NumValueModifier Toughness { get; }
        public CreatureAbilityModifier Ability { get; }
        public Choice Choice { get; }

        public string Name { get; } = null;

        public EffectActionModifyCard(
            Choice Choice,
            NumValueModifier Cost = null,
            NumValueModifier Power = null,
            NumValueModifier Toughness = null,
            CreatureAbilityModifier Ability = null,
            string Name = null
            )
        {
            this.Cost = Cost;
            this.Power = Power;
            this.Toughness = Toughness;
            this.Ability = Ability;
            this.Choice = Choice;
            this.Name = Name;
        }
    }
}
