using MessagePack;
using System;

namespace Cauldron.Shared
{
    [MessagePackObject(true)]
    public struct CardEffectId : IComparable<CardEffectId>, IEquatable<CardEffectId>
    {
        public static bool operator ==(CardEffectId left, CardEffectId right) => left.Value == right.Value;
        public static bool operator !=(CardEffectId left, CardEffectId right) => left.Value != right.Value;

        public static CardEffectId NewId() => new CardEffectId(Guid.NewGuid());
        public static CardEffectId Parse(string input) => new CardEffectId(Guid.Parse(input));

        public Guid Value { get; }

        public CardEffectId(Guid value)
        {
            this.Value = value;
        }

        public override string ToString() => this.Value.ToString();
        public bool Equals(CardEffectId other) => this.Value.Equals(other.Value);
        public override bool Equals(object obj) => (obj is CardEffectId other) && this.Equals(other);
        public override int GetHashCode() => this.Value.GetHashCode();

        public int CompareTo(CardEffectId other) => this.Value.CompareTo(other.Value);
    }
}
