using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public readonly struct PlayerId : IComparable<PlayerId>, IEquatable<PlayerId>
    {
        public static bool operator ==(PlayerId left, PlayerId right) => left.Value == right.Value;
        public static bool operator !=(PlayerId left, PlayerId right) => left.Value != right.Value;

        public static PlayerId NewId() => new PlayerId(Guid.NewGuid());
        public static PlayerId Parse(string input) => new PlayerId(Guid.Parse(input));

        public Guid Value { get; }

        public PlayerId(Guid value)
        {
            this.Value = value;
        }

        public override string ToString() => this.Value.ToString();
        public bool Equals(PlayerId other) => this.Value.Equals(other.Value);
        public override bool Equals(object obj) => (obj is PlayerId other) && this.Equals(other);
        public override int GetHashCode() => this.Value.GetHashCode();

        public int CompareTo(PlayerId other) => this.Value.CompareTo(other.Value);
    }
}
