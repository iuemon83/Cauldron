using System;

namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カードの移動時
    /// </summary>
    public record EffectTimingMoveCardEvent(EffectTimingMoveCardEvent.EventSource Source, ZonePrettyName From, ZonePrettyName To)
    {
        public enum EventSource
        {
            This,
            Other
        }

        public bool IsMatch(Card ownerCard, EffectEventArgs args)
        {
            var matchSource = this.Source switch
            {
                EventSource.This => ownerCard.Id == args.SourceCard.Id,
                EventSource.Other => ownerCard.Id != args.SourceCard.Id,
                _ => throw new InvalidOperationException()
            };

            return matchSource
                && args.GameMaster.ConvertZone(ownerCard.OwnerId, this.From) == args.MoveCardContext.From
                && args.GameMaster.ConvertZone(ownerCard.OwnerId, this.To) == args.MoveCardContext.To;
        }
    }
}
