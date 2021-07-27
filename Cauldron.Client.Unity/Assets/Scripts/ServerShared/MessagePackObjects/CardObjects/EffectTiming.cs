using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTiming
    {
        public EffectTimingStartTurnEvent StartTurn { get; }
        public EffectTimingEndTurnEvent EndTurn { get; }
        public EffectTimingPlayEvent Play { get; }
        public EffectTimingDestroyEvent Destroy { get; }
        public EffectTimingDamageBeforeEvent DamageBefore { get; }
        public EffectTimingDamageAfterEvent DamageAfter { get; }
        public EffectTimingMoveCardEvent MoveCard { get; }

        public EffectTiming(
            EffectTimingStartTurnEvent StartTurn = null,
            EffectTimingEndTurnEvent EndTurn = null,
            EffectTimingPlayEvent Play = null,
            EffectTimingDestroyEvent Destroy = null,
            EffectTimingDamageBeforeEvent DamageBefore = null,
            EffectTimingDamageAfterEvent DamageAfter = null,
            EffectTimingMoveCardEvent MoveCard = null
            )
        {
            this.StartTurn = StartTurn;
            this.EndTurn = EndTurn;
            this.Play = Play;
            this.Destroy = Destroy;
            this.DamageBefore = DamageBefore;
            this.DamageAfter = DamageAfter;
            this.MoveCard = MoveCard;
        }
    }
}
