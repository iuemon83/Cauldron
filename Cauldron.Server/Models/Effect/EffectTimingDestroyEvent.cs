using System;

namespace Cauldron.Server.Models.Effect
{
    public class EffectTimingDestroyEvent
    {
        public enum EventOwner
        {
            This,
            Other
        }

        public EventOwner Owner { get; set; }

        public bool Match(Card ownerCard, Card source)
        {
            return this.Owner switch
            {
                EventOwner.This => ownerCard.Id == source.Id,
                EventOwner.Other => ownerCard.Id != source.Id,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
