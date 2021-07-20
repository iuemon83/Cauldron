namespace Cauldron.Shared.MessagePackObjects
{
    public static class ZonePrettyNameExtensions
    {
        public static (bool Success, Zone Zone) TryGetZone(this ZonePrettyName zonePrettyName, PlayerId effectOwnerId, PlayerId opponentId, PlayerId cardOwnerId)
        {
            return zonePrettyName switch
            {
                ZonePrettyName.CardPool => (true, new Zone(default, ZoneName.CardPool)),
                ZonePrettyName.YouHand => (true, new Zone(effectOwnerId, ZoneName.Hand)),
                ZonePrettyName.OpponentHand => (true, new Zone(opponentId, ZoneName.Hand)),
                ZonePrettyName.OwnerHand => (true, new Zone(cardOwnerId, ZoneName.Hand)),
                ZonePrettyName.YouField => (true, new Zone(effectOwnerId, ZoneName.Field)),
                ZonePrettyName.OpponentField => (true, new Zone(opponentId, ZoneName.Field)),
                ZonePrettyName.OwnerField => (true, new Zone(cardOwnerId, ZoneName.Field)),
                ZonePrettyName.YouCemetery => (true, new Zone(effectOwnerId, ZoneName.Cemetery)),
                ZonePrettyName.OpponentCemetery => (true, new Zone(opponentId, ZoneName.Cemetery)),
                ZonePrettyName.OwnerCemetery => (true, new Zone(cardOwnerId, ZoneName.Cemetery)),
                ZonePrettyName.YouDeck => (true, new Zone(effectOwnerId, ZoneName.Deck)),
                ZonePrettyName.OpponentDeck => (true, new Zone(opponentId, ZoneName.Deck)),
                ZonePrettyName.OwnerDeck => (true, new Zone(cardOwnerId, ZoneName.Deck)),
                _ => (false, default)
            };
        }
    }
}
