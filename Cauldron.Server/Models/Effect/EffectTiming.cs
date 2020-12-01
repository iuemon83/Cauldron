using System;

namespace Cauldron.Server.Models.Effect
{
    public class EffectTiming
    {
        public EffectTimingStartTurnEvent StartTurn { get; set; }
        public EffectTimingEndTurnEvent EndTurn { get; set; }
        public EffectTimingPlayEvent Play { get; set; }
        public EffectTimingDestroyEvent Destroy { get; set; }
        public EffectTimingDamageBeforeEvent DamageBefore { get; set; }
        public EffectTimingDamageAfterEvent DamageAfter { get; set; }
        public EffectTimingBattleBeforeEvent BattleBefore { get; set; }
        public EffectTimingBattleAfterEvent BattleAfter { get; set; }

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
