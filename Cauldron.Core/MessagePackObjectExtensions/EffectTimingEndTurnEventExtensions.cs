using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingEndTurnEventExtensions
    {
        public static bool IsMatch(this EffectTimingEndTurnEvent effectTimingEndTurnEvent, PlayerId turnPlayerId, Card ownerCard)
        {
            return effectTimingEndTurnEvent.Source switch
            {
                EffectTimingEndTurnEvent.SourceValue.Both => true,
                EffectTimingEndTurnEvent.SourceValue.You => turnPlayerId == ownerCard.OwnerId,
                EffectTimingEndTurnEvent.SourceValue.Opponent => turnPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(effectTimingEndTurnEvent.Source)}={effectTimingEndTurnEvent.Source}"),
            };
        }
    }
}
