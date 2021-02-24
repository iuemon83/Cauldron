using MessagePack;
using System;

namespace Cauldron.Shared
{
    [MessagePackObject(true)]
    public struct CardId : IComparable<CardId>, IEquatable<CardId>
    {
        public static bool operator ==(CardId left, CardId right) => left.Value == right.Value;
        public static bool operator !=(CardId left, CardId right) => left.Value != right.Value;

        public static CardId NewId() => new CardId(Guid.NewGuid());
        public static CardId Parse(string input) => new CardId(Guid.Parse(input));

        public Guid Value { get; }

        public CardId(Guid value)
        {
            this.Value = value;
        }

        public override string ToString() => this.Value.ToString();
        public bool Equals(CardId other) => this.Value.Equals(other.Value);
        public override bool Equals(object obj) => (obj is CardId other) && this.Equals(other);
        public override int GetHashCode() => this.Value.GetHashCode();

        public int CompareTo(CardId other) => this.Value.CompareTo(other.Value);
    }
}
