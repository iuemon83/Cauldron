using System;

namespace Cauldron.Server.Models.Effect
{
    public class EffectTimingPlayEvent
    {
        public enum EventSource
        {
            This,
            Other
        }

        public EventSource Source { get; set; }

        public bool Match(Card ownerCard, Card source)
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
