using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingDestroyEventExtensions
    {
        public static bool IsMatch(this EffectTimingDestroyEvent effectTimingDestroyEvent, Card ownerCard, Card source)
        {
            return effectTimingDestroyEvent.Source switch
            {
                EffectTimingDestroyEvent.EventSource.This => ownerCard.Id == source.Id,
                EffectTimingDestroyEvent.EventSource.Other => ownerCard.Id != source.Id,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
