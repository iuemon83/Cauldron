using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class CreatureAbilityModifier
    {
        public enum OperatorValue
        {
            Add,
            Remove,
            Clear
        }

        public OperatorValue Operator { get; }
        public CreatureAbility Value { get; }

        public CreatureAbilityModifier(
            OperatorValue Operator,
            CreatureAbility Value)
        {
            this.Operator = Operator;
            this.Value = Value;
        }
    }
}
