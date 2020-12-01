namespace Cauldron.Server.Models.Effect
{
    public class EffectTimingBattleBeforeEvent
    {
        public enum EventSource
        {
            All,
            Attack,
            Guard,
        }

        public EventSource Source { get; set; }

        public PlayerCondition PlayerCondition { get; set; }

        public CardCondition CardCondition { get; set; }

        public bool Match(Card effectOwnerCard, EffectEventArgs eventArgs)
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


            //var sourcePlayer = this.Source == EventSource.Attack
            //    ? null
            //    : eventArgs.BattleContext.GuardPlayer;

            //var playerMatch = sourcePlayer != null
            //    && (this.PlayerCondition?.IsMatch(effectOwnerCard, sourcePlayer, eventArgs) ?? true);

            //var sourceCard = this.Source == EventSource.Attack
            //    ? eventArgs.BattleContext.AttackCard
            //    : eventArgs.BattleContext.GuardCard;

            //eventArgs.SourceCard = sourceCard;

            //var cardMatch = sourceCard != null
            //    && (this.CardCondition?.IsMatch(effectOwnerCard, sourceCard, eventArgs) ?? true);

            //return playerMatch || cardMatch;
        }
    }
}
