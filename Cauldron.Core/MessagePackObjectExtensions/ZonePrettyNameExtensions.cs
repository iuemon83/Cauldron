namespace Cauldron.Shared.MessagePackObjects
{
    public static class ZonePrettyNameExtensions
    {
        public static (bool, Zone) TryGetZone(this ZonePrettyName zonePrettyName, PlayerId cardOwnerId, PlayerId opponentId)
        {
            return zonePrettyName switch
            {
                ZonePrettyName.CardPool => (true, new Zone(default, ZoneName.CardPool)),
                ZonePrettyName.YouHand => (true, new Zone(cardOwnerId, ZoneName.Hand)),
                ZonePrettyName.OpponentHand => (true, new Zone(opponentId, ZoneName.Hand)),
                ZonePrettyName.YouField => (true, new Zone(cardOwnerId, ZoneName.Field)),
                ZonePrettyName.OpponentField => (true, new Zone(opponentId, ZoneName.Field)),
                ZonePrettyName.YouCemetery => (true, new Zone(cardOwnerId, ZoneName.Cemetery)),
                ZonePrettyName.OpponentCemetery => (true, new Zone(opponentId, ZoneName.Cemetery)),
                ZonePrettyName.YouDeck => (true, new Zone(cardOwnerId, ZoneName.Deck)),
                ZonePrettyName.OpponentDeck => (true, new Zone(opponentId, ZoneName.Deck)),
                _ => (false, default)
            };
        }
    }
}
