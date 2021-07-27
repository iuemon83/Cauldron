using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingStartTurnEventExtensions
    {
        public static bool IsMatch(this EffectTimingStartTurnEvent effectTimingStartTurnEvent, PlayerId turnPlayerId, Card ownerCard)
        {
            return effectTimingStartTurnEvent.Source switch
            {
                EffectTimingStartTurnEvent.SourceValue.Both => true,
                EffectTimingStartTurnEvent.SourceValue.You => turnPlayerId == ownerCard.OwnerId,
                EffectTimingStartTurnEvent.SourceValue.Opponent => turnPlayerId != ownerCard.OwnerId,
                _ => throw new InvalidOperationException($"{nameof(effectTimingStartTurnEvent.Source)}={effectTimingStartTurnEvent.Source}"),
            };
        }
    }
}
