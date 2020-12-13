using System;

namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カードの移動時
    /// </summary>
    public record EffectTimingMoveCardEvent(EffectTimingMoveCardEvent.EventSource Source, ZoneType From, ZoneType To)
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
                && this.MatchZone(this.From, args.MoveCardContext.From)
                && this.MatchZone(this.To, args.MoveCardContext.To);
        }

        public bool MatchZone(ZoneType left, ZoneType right)
        {
            return left switch
            {
                ZoneType.All => true,
                ZoneType.Field => right switch
                {
                    ZoneType.Field or ZoneType.OpponentField or ZoneType.YouField => true,
                    _ => false
                },
                ZoneType.Cemetery => right switch
                {
                    ZoneType.Cemetery or ZoneType.OpponentCemetery or ZoneType.YouCemetery => true,
                    _ => false
                },
                ZoneType.Deck => right switch
                {
                    ZoneType.Deck or ZoneType.OpponentDeck or ZoneType.YouDeck => true,
                    _ => false
                },
                ZoneType.Hand => right switch
                {
                    ZoneType.Hand or ZoneType.OpponentHand or ZoneType.YouHand => true,
                    _ => false
                },
                _ => left == right
            };
        }
    }
}
