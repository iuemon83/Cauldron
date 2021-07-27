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
                EffectTimingMoveCardEvent.SourceValue.This => ownerCard.Id == args.SourceCard.Id,
                EffectTimingMoveCardEvent.SourceValue.Other => ownerCard.Id != args.SourceCard.Id,
                _ => throw new InvalidOperationException()
            };

            var opponentId = args.GameMaster.GetOpponent(ownerCard.OwnerId).Id;

            var (fromSuccess, fromZone) = effectTimingMoveCardEvent.From.TryGetZone(
                ownerCard.OwnerId,
                opponentId,
                args.SourceCard.OwnerId);
            var (toSuccess, toZone) = effectTimingMoveCardEvent.To.TryGetZone(
                ownerCard.OwnerId,
                opponentId,
                args.SourceCard.OwnerId);

            return matchSource
                && fromSuccess
                && toSuccess
                && fromZone == args.MoveCardContext.From
                && toZone == args.MoveCardContext.To;
        }
    }
}
