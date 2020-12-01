namespace Cauldron.Server.Models.Effect
{
    public record EffectTimingDamageBeforeEvent(
        EffectTimingDamageBeforeEvent.EventSource Source,
        PlayerCondition PlayerCondition = null,
        CardCondition CardCondition = null
        )
    {
        public enum EventSource
        {
            All,
            DamageSource,
            Guard,
        }

        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var playerMatch = this.PlayerIsMatch(effectOwnerCard, eventArgs);

            var cardMatch = this.CardIsMatch(effectOwnerCard, eventArgs);

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
