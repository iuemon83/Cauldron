
using MessagePack;
using System;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class Zone : IEquatable<Zone>
    {
        public PlayerId PlayerId { get; }

        public ZoneName ZoneName { get; }

        public Zone(PlayerId playerId, ZoneName zoneName)
        {
            this.PlayerId = playerId;
            this.ZoneName = zoneName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Zone);
        }

        public bool Equals(Zone other)
        {
            return other != null &&
                   PlayerId.Equals(other.PlayerId) &&
                   ZoneName == other.ZoneName;
        }

        public override int GetHashCode()
        {
            int hashCode = 1789637577;
            hashCode = hashCode * -1521134295 + PlayerId.GetHashCode();
            hashCode = hashCode * -1521134295 + ZoneName.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Zone left, Zone right)
        {
            return EqualityComparer<Zone>.Default.Equals(left, right);
        }

        public static bool operator !=(Zone left, Zone right)
        {
            return !(left == right);
        }
    }
}
