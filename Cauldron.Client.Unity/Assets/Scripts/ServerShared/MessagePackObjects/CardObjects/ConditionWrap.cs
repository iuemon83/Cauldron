using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ConditionWrap
    {
        public ConditionAnd ConditionAnd { get; }
        public ConditionOr ConditionOr { get; }
        public ConditionNot ConditionNot { get; }

        public NumCondition NumCondition { get; }

        public TextCondition TextCondition { get; }

        public PlayerCondition PlayerExistsCondition { get; }

        public ConditionWrap(
            ConditionAnd ConditionAnd = null,
            ConditionOr ConditionOr = null,
            ConditionNot ConditionNot = null,
            NumCondition NumCondition = null,
            TextCondition TextCondition = null,
            PlayerCondition PlayerExistsCondition = null
            )
        {
            this.ConditionAnd = ConditionAnd;
            this.ConditionOr = ConditionOr;
            this.ConditionNot = ConditionNot;
            this.NumCondition = NumCondition;
            this.TextCondition = TextCondition;
            this.PlayerExistsCondition = PlayerExistsCondition;
        }
    }
}
