using System;
using System.Linq;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ZoneExtensions
    {
        public static readonly ZoneName[] PublicZoneNames = new[] { ZoneName.Field, ZoneName.Cemetery, ZoneName.CardPool };

        public static bool IsPublic(this Zone zoneMessage) => PublicZoneNames.Contains(zoneMessage.ZoneName);

        public static ZonePrettyName AsZonePrettyName(this Zone zoneMessage, Card card)
        {
            return zoneMessage.ZoneName switch
            {
                ZoneName.CardPool => ZonePrettyName.CardPool,
                ZoneName.Hand => zoneMessage.PlayerId == card.OwnerId ? ZonePrettyName.YouHand : ZonePrettyName.OpponentHand,
                ZoneName.Field => zoneMessage.PlayerId == card.OwnerId ? ZonePrettyName.YouField : ZonePrettyName.OpponentField,
                ZoneName.Cemetery => zoneMessage.PlayerId == card.OwnerId ? ZonePrettyName.YouCemetery : ZonePrettyName.OpponentCemetery,
                ZoneName.Deck => zoneMessage.PlayerId == card.OwnerId ? ZonePrettyName.YouDeck : ZonePrettyName.OpponentDeck,
                _ => throw new InvalidOperationException()
            };
        }
    }
}
