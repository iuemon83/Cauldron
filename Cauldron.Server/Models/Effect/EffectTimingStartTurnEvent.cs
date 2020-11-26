using System;

namespace Cauldron.Server.Models.Effect
{
    public class EffectTimingStartTurnEvent
    {
        public enum EventSource
        {
            Owner,
            Other,
            Both
        }

        public EventSource Source { get; set; }

        public bool Match(Guid turnPlayerId, Card ownerCard)
        {
            return this.Source switch
            {
                EventSource.Both => true,
                EventSource.Owner => turnPlayerId == ownerCard.OwnerId,
                EventSource.Other => turnPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(this.Source)}={this.Source}"),
            };
        }
    }
}
