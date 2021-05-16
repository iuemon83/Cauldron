using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class ZonePrettyNameExtensions
    {
        public static Zone FromPrettyName(this ZonePrettyName zonePrettyName, PlayerId cardOwnerId, PlayerId opponentId)
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
    }
}
