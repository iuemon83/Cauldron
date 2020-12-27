using System;

namespace Cauldron.Server.Models
{
    public readonly struct CardDefId : IComparable<CardDefId>, IEquatable<CardDefId>
    {
        public static bool operator ==(CardDefId left, CardDefId right) => left.Value == right.Value;
        public static bool operator !=(CardDefId left, CardDefId right) => left.Value != right.Value;

        public static CardDefId NewId() => new CardDefId(Guid.NewGuid());
        public static CardDefId Parse(string input) => new CardDefId(Guid.Parse(input));

        public Guid Value { get; }

        public CardDefId(Guid value)
        {
            this.Value = value;
        }

        public override string ToString() => this.Value.ToString();
        public bool Equals(CardDefId other) => this.Value.Equals(other.Value);
        public override bool Equals(object obj) => (obj is CardDefId other) && this.Equals(other);
        public override int GetHashCode() => this.Value.GetHashCode();

        public int CompareTo(CardDefId other) => this.Value.CompareTo(other.Value);
    }
}
