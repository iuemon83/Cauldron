using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class CreatureAbilityModifier
    {
        public enum OperatorValue
        {
            [DisplayText("追加")]
            Add,
            [DisplayText("削除")]
            Remove,
            [DisplayText("全削除")]
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
