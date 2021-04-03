using Cauldron.Core.Entities.Effect;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectTimingMoveCardEventExntensions
    {
        public static bool IsMatch(this EffectTimingMoveCardEvent effectTimingMoveCardEvent, Card ownerCard, EffectEventArgs args)
        {
            var matchSource = effectTimingMoveCardEvent.Source switch
            {
                EffectTimingMoveCardEvent.EventSource.This => ownerCard.Id == args.SourceCard.Id,
                EffectTimingMoveCardEvent.EventSource.Other => ownerCard.Id != args.SourceCard.Id,
                _ => throw new InvalidOperationException()
            };

            var opponentId = args.GameMaster.GetOpponent(ownerCard.OwnerId).Id;

            return matchSource
                && effectTimingMoveCardEvent.From.FromPrettyName(ownerCard.OwnerId, opponentId) == args.MoveCardContext.From
                && effectTimingMoveCardEvent.To.FromPrettyName(ownerCard.OwnerId, opponentId) == args.MoveCardContext.To;
        }
    }
}
