using System;

namespace Cauldron.Server.Models.Effect
{
    public record EffectTimingDestroyEvent(EffectTimingDestroyEvent.EventSource Source)
    {
        public enum EventSource
        {
            This,
            Other
        }

        public bool IsMatch(Card ownerCard, Card source)
        {
            return this.Source switch
            {
                EventSource.This => ownerCard.Id == source.Id,
                EventSource.Other => ownerCard.Id != source.Id,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
