namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード効果を発動するための条件
    /// </summary>
    public record EffectCondition(
        ZonePrettyName ZonePrettyName,
        EffectWhen When,
        EffectWhile While = null,
        EffectIf If = null
        )
    {
        public static readonly EffectCondition Spell
            = new(ZonePrettyName.YouField, new(new(Play: new(EffectTimingPlayEvent.EventSource.This))));

        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return this.IsMatchedZone(effectOwnerCard, eventArgs)
                && (this.While?.IsMatch(effectOwnerCard, eventArgs) ?? true)
                && this.When.IsMatch(effectOwnerCard, eventArgs)
                && (this.If?.IsMatch(effectOwnerCard, eventArgs) ?? true);
        }

        private bool IsMatchedZone(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var zone = Zone.FromPrettyName(effectOwnerCard.OwnerId, eventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId).Id, this.ZonePrettyName);
            return effectOwnerCard.Zone == zone;
        }
    }
}
