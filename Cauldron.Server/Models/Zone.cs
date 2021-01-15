using System;
using System.Linq;

namespace Cauldron.Server.Models
{
    public record Zone(PlayerId PlayerId, ZoneName ZoneName)
    {
        public static Zone FromPrettyName(PlayerId cardOwnerId, PlayerId opponentId, ZonePrettyName zonePrettyName)
        {
            return zonePrettyName switch
            {
                ZonePrettyName.CardPool => new Zone(default, ZoneName.CardPool),
                ZonePrettyName.YouHand => new Zone(cardOwnerId, ZoneName.Hand),
                ZonePrettyName.OpponentHand => new Zone(opponentId, ZoneName.Hand),
                ZonePrettyName.YouField => new Zone(cardOwnerId, ZoneName.Field),
                ZonePrettyName.OpponentField => new Zone(opponentId, ZoneName.Field),
                ZonePrettyName.YouCemetery => new Zone(cardOwnerId, ZoneName.Cemetery),
                ZonePrettyName.OpponentCemetery => new Zone(opponentId, ZoneName.Cemetery),
                ZonePrettyName.YouDeck => new Zone(cardOwnerId, ZoneName.Deck),
                ZonePrettyName.OpponentDeck => new Zone(opponentId, ZoneName.Deck),
                _ => throw new InvalidOperationException()
            };
        }

        public static readonly ZoneName[] PublicZoneNames = new[] { ZoneName.Field, ZoneName.Cemetery, ZoneName.CardPool };

        public bool IsPublic => PublicZoneNames.Contains(this.ZoneName);

        public ZonePrettyName AsZonePrettyName(Card card)
        {
            return this.ZoneName switch
            {
                ZoneName.CardPool => ZonePrettyName.CardPool,
                ZoneName.Hand => this.PlayerId == card.OwnerId ? ZonePrettyName.YouHand : ZonePrettyName.OpponentHand,
                ZoneName.Field => this.PlayerId == card.OwnerId ? ZonePrettyName.YouField : ZonePrettyName.OpponentField,
                ZoneName.Cemetery => this.PlayerId == card.OwnerId ? ZonePrettyName.YouCemetery : ZonePrettyName.OpponentCemetery,
                ZoneName.Deck => this.PlayerId == card.OwnerId ? ZonePrettyName.YouDeck : ZonePrettyName.OpponentDeck,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
