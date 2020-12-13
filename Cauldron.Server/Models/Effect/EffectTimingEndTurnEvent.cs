using System;

namespace Cauldron.Server.Models.Effect
{
    public record EffectTimingEndTurnEvent(EffectTimingEndTurnEvent.EventSource Source)
    {
        public enum EventSource
        {
            You,
            Opponent,
            Both
        }

        public bool IsMatch(Guid turnPlayerId, Card ownerCard)
        {
            return this.Source switch
            {
                EventSource.Both => true,
                EventSource.You => turnPlayerId == ownerCard.OwnerId,
                EventSource.Opponent => turnPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(this.Source)}={this.Source}"),
            };
        }
    }
}
