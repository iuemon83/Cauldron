#nullable enable

using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectActionModifyCard
    {
        public NumValueModifier? Cost { get; }
        public NumValueModifier? Power { get; }
        public NumValueModifier? Toughness { get; }
        public CreatureAbilityModifier? Ability { get; }
        public AnnotationsModifier? Annotations { get; }
        public NumValueModifier? NumTurnsToCanAttack { get; }
        public Choice Choice { get; }

        public string? Name { get; } = null;

        public EffectActionModifyCard(
            Choice Choice,
            NumValueModifier? Cost = null,
            NumValueModifier? Power = null,
            NumValueModifier? Toughness = null,
            CreatureAbilityModifier? Ability = null,
            AnnotationsModifier? Annotations = null,
            NumValueModifier? NumTurnsToCanAttack = null,
            string? Name = null
            )
        {
            this.Cost = Cost;
            this.Power = Power;
            this.Toughness = Toughness;
            this.Ability = Ability;
            this.Annotations = Annotations;
            this.Choice = Choice;
            this.NumTurnsToCanAttack = NumTurnsToCanAttack;
            this.Name = Name;
        }
    }
}
