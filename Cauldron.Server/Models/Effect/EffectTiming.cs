using System;

namespace Cauldron.Server.Models.Effect
{
    public class EffectTiming
    {
        public EffectTimingPlayEvent Play { get; set; }
        public EffectTimingDestroyEvent Destroy { get; set; }
        public EffectTimingStartTurnEvent StartTurn { get; set; }
        public EffectTimingEndTurnEvent EndTurn { get; set; }

        public bool Match(GameEvent effectType, Guid turnPlayerId, Card ownerCard, Card source)
        {
            return effectType switch
            {
                GameEvent.OnPlay => this.Play?.Match(ownerCard, source) ?? false,
                //CardEffectType.OnEveryPlay => this.Play?.Owner == EffectTimingPlayEvent.EventOwner.Other,
                GameEvent.OnDestroy => this.Destroy?.Match(ownerCard, source) ?? false,
                //CardEffectType.OnEveryDestroy => this.Destroy?.Owner == EffectTimingDestroyEvent.EventOwner.Other,
                GameEvent.OnStartTurn => this.StartTurn?.Match(turnPlayerId, ownerCard) ?? false,
                GameEvent.OnEndTurn => this.EndTurn?.Match(turnPlayerId, ownerCard) ?? false,
                _ => false,
            };
        }
    }
}
