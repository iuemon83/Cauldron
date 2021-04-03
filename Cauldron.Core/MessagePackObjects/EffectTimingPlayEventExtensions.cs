using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingPlayEventExtensions
    {
        public static bool IsMatch(this EffectTimingPlayEvent effectTimingPlayEvent, Card ownerCard, Card source)
        {
            return effectTimingPlayEvent.Source switch
            {
                EffectTimingPlayEvent.EventSource.This => ownerCard.Id == source.Id,
                EffectTimingPlayEvent.EventSource.Other => ownerCard.Id != source.Id,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
