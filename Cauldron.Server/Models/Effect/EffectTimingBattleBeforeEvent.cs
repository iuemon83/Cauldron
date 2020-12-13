namespace Cauldron.Server.Models.Effect
{
    public record EffectTimingBattleBeforeEvent(
        EffectTimingBattleBeforeEvent.EventSource Source,
        PlayerCondition PlayerCondition = null,
        CardCondition CardCondition = null
        )
    {
        public enum EventSource
        {
            All,
            Attack,
            Guard,
        }

        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var playerMatch = this.Source switch
            {
                EventSource.All => (this.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.GuardPlayer, eventArgs) ?? false),
                EventSource.Guard => this.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.GuardPlayer, eventArgs) ?? false,
                _ => false
            };

            var cardMatch = this.Source switch
            {
                EventSource.All =>
                    (this.CardCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.AttackCard, eventArgs) ?? false)
                    || (this.CardCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.GuardCard, eventArgs) ?? false)
                ,
                EventSource.Attack => this.CardCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.AttackCard, eventArgs) ?? false,
                EventSource.Guard => this.CardCondition?.IsMatch(effectOwnerCard, eventArgs.BattleContext.GuardCard, eventArgs) ?? false,
                _ => false
            };

            return playerMatch || cardMatch;
        }
    }
}
