using System;
using System.Linq;

namespace Cauldron.Server.Models
{
    public record Zone(PlayerId PlayerId, ZoneName ZoneName)
    {
        public static readonly ZoneName[] PublicZoneNames = new[] { ZoneName.Field, ZoneName.Cemetery, ZoneName.CardPool };

        public bool IsPublic => PublicZoneNames.Contains(this.ZoneName);
    }
}
