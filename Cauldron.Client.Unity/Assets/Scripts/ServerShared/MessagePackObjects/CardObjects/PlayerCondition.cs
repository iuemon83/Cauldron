using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PlayerCondition
    {
        public enum PlayerConditionContext
        {
            All,
            EventSource
        }

        public enum PlayerConditionType
        {
            All,
            You,
            Opponent,
            Active,
            NonActive,
        }

        public PlayerCondition.PlayerConditionContext Context { get; set; }
        public PlayerCondition.PlayerConditionType Type { get; set; }

        public PlayerCondition(
            PlayerCondition.PlayerConditionContext Context = PlayerConditionContext.All,
            PlayerCondition.PlayerConditionType Type = PlayerConditionType.All)
        {
            this.Context = Context;
            this.Type = Type;
        }
    }
}