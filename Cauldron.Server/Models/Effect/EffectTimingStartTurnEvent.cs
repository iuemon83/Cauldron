﻿using System;

namespace Cauldron.Server.Models.Effect
{
    public record EffectTimingStartTurnEvent(EffectTimingStartTurnEvent.EventSource Source)
    {
        public enum EventSource
        {
            Owner,
            Other,
            Both
        }

        public bool IsMatch(PlayerId turnPlayerId, Card ownerCard)
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