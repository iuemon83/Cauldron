using System;

namespace Cauldron.Server.Models.Effect
{
    public record EffectTiming(
        EffectTimingStartTurnEvent StartTurn = null,
        EffectTimingEndTurnEvent EndTurn = null,
        EffectTimingPlayEvent Play = null,
        EffectTimingDestroyEvent Destroy = null,
        EffectTimingDamageBeforeEvent DamageBefore = null,
        EffectTimingDamageAfterEvent DamageAfter = null,
        EffectTimingBattleBeforeEvent BattleBefore = null,
        EffectTimingBattleAfterEvent BattleAfter = null
        )
    {
        public bool Match(GameEvent gameEvent)
        {
            return gameEvent switch
            {
                GameEvent.OnStartTurn => this.StartTurn != null,
                GameEvent.OnEndTurn => this.EndTurn != null,
                GameEvent.OnPlay => this.Play != null,
                GameEvent.OnDestroy => this.Destroy != null,
                GameEvent.OnDamageBefore => this.DamageBefore != null,
                GameEvent.OnDamage => this.DamageAfter != null,
                GameEvent.OnBattleBefore => this.BattleBefore != null,
                GameEvent.OnBattle => this.BattleAfter != null,
                _ => throw new InvalidOperationException()
            };
        }

        public bool Match(GameEvent effectType, Guid turnPlayerId, Card ownerCard, EffectEventArgs eventArgs)
        {
            return effectType switch
            {
                GameEvent.OnStartTurn => this.StartTurn?.Match(turnPlayerId, ownerCard) ?? false,
                GameEvent.OnEndTurn => this.EndTurn?.Match(turnPlayerId, ownerCard) ?? false,
                GameEvent.OnPlay => this.Play?.Match(ownerCard, eventArgs.SourceCard) ?? false,
                GameEvent.OnDestroy => this.Destroy?.Match(ownerCard, eventArgs.SourceCard) ?? false,
                GameEvent.OnDamageBefore => this.DamageBefore?.IsMatch(ownerCard, eventArgs) ?? false,
                GameEvent.OnDamage => this.DamageAfter?.IsMatch(ownerCard, eventArgs) ?? false,
                GameEvent.OnBattleBefore => this.BattleBefore?.Match(ownerCard, eventArgs) ?? false,
                GameEvent.OnBattle => this.BattleAfter?.Match(ownerCard, eventArgs) ?? false,
                _ => false,
            };
        }
    }
}
