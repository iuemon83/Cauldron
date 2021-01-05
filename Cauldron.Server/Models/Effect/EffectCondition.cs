namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード効果を発動するための条件
    /// </summary>
    public record EffectCondition(
        ZonePrettyName Zone,
        EffectWhen When,
        EffectWhile While = null
        )
    {
        public static readonly EffectCondition Spell
            = new(ZonePrettyName.YouField, new(new(Play: new(EffectTimingPlayEvent.EventSource.This))));

        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            return this.IsMatchedZone(effectOwnerCard, eventArgs)
                && (this.While?.IsMatch(effectOwnerCard, eventArgs) ?? true)
                && this.When.IsMatch(effectOwnerCard, eventArgs);
        }

        private bool IsMatchedZone(Card effectOwnerCard, EffectEventArgs eventArgs)
            => effectOwnerCard.Zone == eventArgs.GameMaster.ConvertZone(effectOwnerCard.OwnerId, this.Zone);
    }
}
