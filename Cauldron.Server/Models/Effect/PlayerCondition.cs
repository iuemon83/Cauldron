namespace Cauldron.Server.Models.Effect
{
    public record PlayerCondition(
        PlayerCondition.PlayerConditionContext Context = PlayerCondition.PlayerConditionContext.All,
        PlayerCondition.PlayerConditionType Type = PlayerCondition.PlayerConditionType.All
        )
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

        public bool IsMatch(Card effectOwnerCard, Player player, EffectEventArgs eventArgs)
        {
            return
                this.Context switch
                {
                    PlayerConditionContext.EventSource => player.Id == eventArgs.SourcePlayer.Id,
                    _ => true
                }
                && this.Type switch
                {
                    PlayerConditionType.You => player.Id == effectOwnerCard.OwnerId,
                    PlayerConditionType.Opponent => player.Id != effectOwnerCard.OwnerId,
                    PlayerConditionType.Active => player.Id == eventArgs.GameMaster.ActivePlayer.Id,
                    PlayerConditionType.NonActive => player.Id != eventArgs.GameMaster.ActivePlayer.Id,
                    _ => true
                };
        }
    }
}