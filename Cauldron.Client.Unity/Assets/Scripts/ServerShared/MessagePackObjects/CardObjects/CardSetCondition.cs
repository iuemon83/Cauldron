using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CardSetCondition
    {
        public enum ConditionType
        {
            [DisplayText("このセット")]
            This,
            [DisplayText("その他のセット")]
            Other
        }

        public ConditionType Type { get; } = ConditionType.This;

        public TextCondition ValueCondition { get; }

        public CardSetCondition(ConditionType Type, TextCondition ValueCondition = null)
        {
            this.Type = Type;
            this.ValueCondition = ValueCondition;
        }
    }
}
