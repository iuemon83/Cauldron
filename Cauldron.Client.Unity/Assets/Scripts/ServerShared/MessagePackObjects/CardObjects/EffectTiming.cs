using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTiming
    {
        public EffectTimingStartTurnEvent StartTurn { get; set; }
        public EffectTimingEndTurnEvent EndTurn { get; set; }
        public EffectTimingPlayEvent Play { get; set; }
        public EffectTimingDestroyEvent Destroy { get; set; }
        public EffectTimingDamageBeforeEvent DamageBefore { get; set; }
        public EffectTimingDamageAfterEvent DamageAfter { get; set; }
        public EffectTimingMoveCardEvent MoveCard { get; set; }

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
