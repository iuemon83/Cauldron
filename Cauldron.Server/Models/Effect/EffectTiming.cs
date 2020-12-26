namespace Cauldron.Server.Models.Effect
{
    public record EffectTiming(
        ZoneType Zone,
        EffectTimingStartTurnEvent StartTurn = null,
        EffectTimingEndTurnEvent EndTurn = null,
        EffectTimingPlayEvent Play = null,
        EffectTimingDestroyEvent Destroy = null,
        EffectTimingDamageBeforeEvent DamageBefore = null,
        EffectTimingDamageAfterEvent DamageAfter = null,
        EffectTimingBattleBeforeEvent BattleBefore = null,
        EffectTimingBattleAfterEvent BattleAfter = null,
        EffectTimingMoveCardEvent MoveCard = null
        )
    {
        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            var isMatchedZone = effectOwnerCard.Zone == eventArgs.GameMaster.ConvertZone(effectOwnerCard.OwnerId, this.Zone);

            return isMatchedZone
                && eventArgs.GameEvent switch
                {
                    GameEvent.OnStartTurn => this.StartTurn?.IsMatch(eventArgs.GameMaster.ActivePlayer.Id, effectOwnerCard) ?? false,
                    GameEvent.OnEndTurn => this.EndTurn?.IsMatch(eventArgs.GameMaster.ActivePlayer.Id, effectOwnerCard) ?? false,
                    GameEvent.OnPlay => this.Play?.IsMatch(effectOwnerCard, eventArgs.SourceCard) ?? false,
                    GameEvent.OnDestroy => this.Destroy?.IsMatch(effectOwnerCard, eventArgs.SourceCard) ?? false,
                    GameEvent.OnDamageBefore => this.DamageBefore?.IsMatch(effectOwnerCard, eventArgs) ?? false,
                    GameEvent.OnDamage => this.DamageAfter?.IsMatch(effectOwnerCard, eventArgs) ?? false,
                    GameEvent.OnBattleBefore => this.BattleBefore?.IsMatch(effectOwnerCard, eventArgs) ?? false,
                    GameEvent.OnBattle => this.BattleAfter?.IsMatch(effectOwnerCard, eventArgs) ?? false,
                    GameEvent.OnMoveCard => this.MoveCard?.IsMatch(effectOwnerCard, eventArgs) ?? false,
                    _ => false,
                };
        }
    }
}
