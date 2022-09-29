using MessagePack;
using System;
using System.Text.Json.Serialization;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public readonly struct GameId : IComparable<GameId>, IEquatable<GameId>
    {
        public static bool operator ==(GameId left, GameId right) => left.Value == right.Value;
        public static bool operator !=(GameId left, GameId right) => left.Value != right.Value;

        public static GameId NewId() => new GameId(Guid.NewGuid());
        public static GameId Parse(string input) => new GameId(Guid.Parse(input));

        public Guid Value { get; }

        [JsonConstructor]
        public GameId(Guid value)
        {
            this.Value = value;
        }

        public override string ToString() => this.Value.ToString();
        public bool Equals(GameId other) => this.Value.Equals(other.Value);
        public override bool Equals(object obj) => (obj is GameId other) && this.Equals(other);
        public override int GetHashCode() => this.Value.GetHashCode();

        public int CompareTo(GameId other) => this.Value.CompareTo(other.Value);
    }
}
