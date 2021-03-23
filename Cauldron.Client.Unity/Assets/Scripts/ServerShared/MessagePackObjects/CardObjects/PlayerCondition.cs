using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PlayerCondition
    {
        public enum PlayerConditionContext
        {
            Any,
            EventSource
        }

        public enum PlayerConditionType
        {
            Any,
            You,
            Opponent,
            Active,
            NonActive,
        }

        public PlayerCondition.PlayerConditionContext Context { get; set; }
        public PlayerCondition.PlayerConditionType Type { get; set; }

        public PlayerCondition(
            PlayerCondition.PlayerConditionContext Context = PlayerConditionContext.Any,
            PlayerCondition.PlayerConditionType Type = PlayerConditionType.Any)
        {
            this.Context = Context;
            this.Type = Type;
        }
    }
}