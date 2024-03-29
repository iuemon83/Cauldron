﻿#nullable enable

using MessagePack;
using System.Collections.Generic;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// プレイヤーの公開情報
    /// </summary>
    [MessagePackObject(true)]
    public class PublicPlayerInfo
    {
        public PlayerId Id { get; }

        /// <summary>
        /// リプレイ用の匿名に置き換えるためにprivate set になっている
        /// recordに変更できたら削除する
        /// </summary>
        public string Name { get; private set; }
        public Card?[] Field { get; }
        public bool[] IsAvailableFields { get; }
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
        public int TurnCount { get; }

        public PublicPlayerInfo(
            PlayerId Id,
            string Name,
            Card?[] Field,
            bool[] IsAvailableFields,
            int DeckCount,
            Card[] Cemetery,
            CardDef[] Excluded,
            int HandsCount,
            int MaxHp,
            int CurrentHp,
            int MaxMp,
            int CurrentMp,
            bool IsFirst,
            Dictionary<CardId, AttackTarget> AttackableCardIdList,
            int TurnCount
            )
        {
            this.Id = Id;
            this.Name = Name;
            this.Field = Field;
            this.IsAvailableFields = IsAvailableFields;
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
            this.TurnCount = TurnCount;
        }

        /// <summary>
        /// 名前をリプレイ用の匿名に置き換えるためのもの
        /// それ以外の用途では利用しない
        /// recordに変更できたら削除する
        /// </summary>
        /// <param name="alias"></param>
        public void SetReplayAliasName(string alias)
        {
            this.Name = alias;
        }
    }
}
