#nullable enable

using MessagePack;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class AttackTarget
    {
        public PlayerId[] PlayerIdList { get; }
        public CardId[] CardIdList { get; }

        public AttackTarget(PlayerId[] playerIdList, CardId[] cardIdList)
        {
            PlayerIdList = playerIdList;
            CardIdList = cardIdList;
        }

        public bool Any => this.PlayerIdList.Any() || this.CardIdList.Any();
    }

    /// <summary>
    /// プレイヤーの公開情報
    /// </summary>
    [MessagePackObject(true)]
    public class PublicPlayerInfo
    {
        public PlayerId Id { get; }
        public string Name { get; }
        public Card?[] Field { get; }
        public int DeckCount { get; }
        public Card[] Cemetery { get; }
        public CardDef[] Excluded { get; }
        public int HandsCount { get; }
        public int MaxHp { get; }
        public int CurrentHp { get; }
        public int MaxMp { get; }
        public int CurrentMp { get; }
        public bool IsFirst { get; }
        public Dictionary<CardId, AttackTarget> AttackableCardIdList { get; }

        public PublicPlayerInfo(
            PlayerId Id,
            string Name,
            Card?[] Field,
            int DeckCount,
            Card[] Cemetery,
            CardDef[] Excluded,
            int HandsCount,
            int MaxHp,
            int CurrentHp,
            int MaxMp,
            int CurrentMp,
            bool IsFirst,
            Dictionary<CardId, AttackTarget> AttackableCardIdList
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
            this.AttackableCardIdList = AttackableCardIdList;
        }
    }
}
