namespace Cauldron.Server.Models.Effect
{
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
            Owner,
            NotOwner,
            Active,
            NonActive,
        }

        public PlayerConditionContext Context { get; set; } = PlayerConditionContext.All;

        public PlayerConditionType Type { get; set; } = PlayerConditionType.All;

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
                    PlayerConditionType.Owner => player.Id == effectOwnerCard.OwnerId,
                    PlayerConditionType.NotOwner => player.Id != effectOwnerCard.OwnerId,
                    PlayerConditionType.Active => player.Id == eventArgs.GameMaster.ActivePlayer.Id,
                    PlayerConditionType.NonActive => player.Id != eventArgs.GameMaster.ActivePlayer.Id,
                    _ => true
                };
        }
    }
}