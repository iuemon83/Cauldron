namespace Cauldron.Server.Models.Effect
{
    public class EffectTimingDamageBeforeEvent
    {
        public enum EventSource
        {
            All,
            DamageSource,
            Guard,
        }

        public EventSource Source { get; set; }

        public PlayerCondition PlayerCondition { get; set; }

        public CardCondition CardCondition { get; set; }

        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var playerMatch = this.PlayerIsMatch(effectOwnerCard, eventArgs);

            var cardMatch = this.CardIsMatch(effectOwnerCard, eventArgs);

            //var playerMatch = sourcePlayer != null
            //    && (this.PlayerCondition?.IsMatch(effectOwnerCard, sourcePlayer, eventArgs) ?? false);

            //var sourceCard = this.Source == EventSource.Attack
            //    ? eventArgs.DamageContext.AttackCard
            //    : eventArgs.DamageContext.GuardCard;

            //eventArgs.SourceCard = sourceCard;

            //var cardMatch = sourceCard != null
            //    && (this.CardCondition?.IsMatch(effectOwnerCard, sourceCard, eventArgs) ?? false);

            return playerMatch || cardMatch;
        }

        private bool PlayerIsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return this.Source switch
            {
                EventSource.All => eventArgs.DamageContext.GuardPlayer != null
                    && (this.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs.DamageContext.GuardPlayer, eventArgs) ?? false),
                EventSource.Guard => eventArgs.DamageContext.GuardPlayer != null
                    && (this.PlayerCondition?.IsMatch(effectOwnerCard, eventArgs.DamageContext.GuardPlayer, eventArgs) ?? false),
                _ => false
            };
        }

        private bool CardIsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var damageSource = eventArgs.DamageContext.DamageSourceCard;
            var guard = eventArgs.DamageContext.GuardCard;

            return this.Source switch
            {
                EventSource.All =>
                    (damageSource != null
                        && (this.CardCondition?.IsMatch(effectOwnerCard, damageSource, eventArgs) ?? false))
                    || (guard != null
                        && (this.CardCondition?.IsMatch(effectOwnerCard, guard, eventArgs) ?? false))
                ,
                EventSource.DamageSource => damageSource != null
                    && (this.CardCondition?.IsMatch(effectOwnerCard, damageSource, eventArgs) ?? false),
                EventSource.Guard => guard != null
                    && (this.CardCondition?.IsMatch(effectOwnerCard, guard, eventArgs) ?? false),
                _ => false
            };
        }
    }
}
