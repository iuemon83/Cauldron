using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// プレイヤーの公開情報
    /// </summary>
    [MessagePackObject(true)]
    public class PublicPlayerInfo
    {
        public PlayerId Id { get; }
        public string Name { get; }
        public Card[] Field { get; }
        public int DeckCount { get; }
        public Card[] Cemetery { get; }
        public CardDef[] Excluded { get; }
        public int HandsCount { get; }
        public int MaxHp { get; }
        public int CurrentHp { get; }
        public int MaxMp { get; }
        public int CurrentMp { get; }
        public bool IsFirst { get; }

        public PublicPlayerInfo(
            PlayerId Id,
            string Name,
            Card[] Field,
            int DeckCount,
            Card[] Cemetery,
            CardDef[] Excluded,
            int HandsCount,
            int MaxHp,
            int CurrentHp,
            int MaxMp,
            int CurrentMp,
            bool IsFirst
            )
        {
            this.Id = Id;
            this.Name = Name;
            this.Field = Field;
            this.DeckCount = DeckCount;
            this.Cemetery = Cemetery;
            this.Excluded = Excluded;
            this.HandsCount = HandsCount;
            this.MaxHp = MaxHp;
            this.CurrentHp = CurrentHp;
            this.MaxMp = MaxMp;
            this.CurrentMp = CurrentMp;
            this.IsFirst = IsFirst;
        }
    }
}
