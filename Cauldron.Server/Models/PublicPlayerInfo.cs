using System;

namespace Cauldron.Server.Models
{
    public class PublicPlayerInfo
    {
        public Guid Id { get; }

        public string Name { get; } = "";

        public Field Field { get; }

        public int DeckCount { get; }

        public int Hp { get; private set; }
        public int MaxMp { get; private set; }
        public int UsedMp { get; private set; }
        public int UsableMp => Math.Max(0, this.MaxMp - this.UsedMp);

        public PublicPlayerInfo(Player player)
        {
            this.Id = player.Id;
            this.Name = player.Name;
            this.Field = player.Field;
            this.Hp = player.Hp;
            this.MaxMp = player.MaxMp;
            this.UsedMp = player.UsedMp;

            this.DeckCount = player.Deck.Count;
        }

        public PublicPlayerInfo(Guid id, string name, Field field, int deckCount, int hp, int maxMp, int usedMp)
        {
            this.Id = id;
            this.Name = name;
            this.Field = field;
            this.DeckCount = deckCount;
            this.Hp = hp;
            this.MaxMp = maxMp;
            this.UsedMp = usedMp;
        }
    }
}
