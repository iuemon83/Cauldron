#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTiming
    {
        public EffectTimingStartTurnEvent? StartTurn { get; }
        public EffectTimingEndTurnEvent? EndTurn { get; }
        public EffectTimingPlayEvent? Play { get; }
        public EffectTimingDestroyEvent? Destroy { get; }
        public EffectTimingAttackBeforeEvent? AttackBefore { get; }
        public EffectTimingAttackAfterEvent? AttackAfter { get; }
        public EffectTimingDamageBeforeEvent? DamageBefore { get; }
        public EffectTimingDamageAfterEvent? DamageAfter { get; }
        public EffectTimingMoveCardEvent? MoveCard { get; }
        public EffectTimingExcludeCardEvent? ExcludeCard { get; }
        public EffectTimingModifyCounterOnCardEvent? ModifyCounter { get; }
        public EffectTimingModifyPlayerEvent? ModifyPlayer { get; }

        public EffectTiming(
            EffectTimingStartTurnEvent? StartTurn = null,
            EffectTimingEndTurnEvent? EndTurn = null,
            EffectTimingPlayEvent? Play = null,
            EffectTimingDestroyEvent? Destroy = null,
            EffectTimingAttackBeforeEvent? AttackBefore = null,
            EffectTimingAttackAfterEvent? AttackAfter = null,
            EffectTimingDamageBeforeEvent? DamageBefore = null,
            EffectTimingDamageAfterEvent? DamageAfter = null,
            EffectTimingMoveCardEvent? MoveCard = null,
            EffectTimingExcludeCardEvent? ExcludeCard = null,
            EffectTimingModifyCounterOnCardEvent? ModifyCounter = null,
            EffectTimingModifyPlayerEvent? ModifyPlayer = null
            )
        {
            this.StartTurn = StartTurn;
            this.EndTurn = EndTurn;
            this.Play = Play;
            this.Destroy = Destroy;
            this.AttackBefore = AttackBefore;
            this.AttackAfter = AttackAfter;
            this.DamageBefore = DamageBefore;
            this.DamageAfter = DamageAfter;
            this.MoveCard = MoveCard;
            this.ExcludeCard = ExcludeCard;
            this.ModifyCounter = ModifyCounter;
            this.ModifyPlayer = ModifyPlayer;
        }
    }
}
