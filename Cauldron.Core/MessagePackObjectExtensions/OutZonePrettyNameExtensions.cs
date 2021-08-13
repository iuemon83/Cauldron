namespace Cauldron.Shared.MessagePackObjects
{
    public static class OutZonePrettyNameExtensions
    {
        public static (bool Success, Zone Zone) TryGetZone(this OutZonePrettyName _this, PlayerId effectOwnerId, PlayerId opponentId, PlayerId cardOwnerId)
        {
            return _this switch
            {
                OutZonePrettyName.CardPool => (true, new Zone(default, ZoneName.CardPool)),
                OutZonePrettyName.YouExcluded => (true, new Zone(effectOwnerId, ZoneName.Excluded)),
                OutZonePrettyName.OpponentExcluded => (true, new Zone(opponentId, ZoneName.Excluded)),
                OutZonePrettyName.OwnerExcluded => (true, new Zone(cardOwnerId, ZoneName.Excluded)),
                _ => (false, default)
            };
        }
    }
}
