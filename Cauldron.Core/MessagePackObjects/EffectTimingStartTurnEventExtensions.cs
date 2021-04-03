using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingStartTurnEventExtensions
    {
        public static bool IsMatch(this EffectTimingStartTurnEvent effectTimingStartTurnEvent, PlayerId turnPlayerId, Card ownerCard)
        {
            return effectTimingStartTurnEvent.Source switch
            {
                EffectTimingStartTurnEvent.EventSource.Both => true,
                EffectTimingStartTurnEvent.EventSource.You => turnPlayerId == ownerCard.OwnerId,
                EffectTimingStartTurnEvent.EventSource.Opponent => turnPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(effectTimingStartTurnEvent.Source)}={effectTimingStartTurnEvent.Source}"),
            };
        }
    }
}
