using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ChoiceCandidates
    {
        public PlayerId[] PlayerIdList { get; }
        public Card[] CardList { get; }
        public CardDef[] CardDefList { get; }

        public int Count => this.PlayerIdList.Length
            + this.CardList.Length
            + this.CardDefList.Length;

        public ChoiceCandidates(PlayerId[] PlayerIdList, Card[] CardList, CardDef[] CardDefList)
        {
            this.PlayerIdList = PlayerIdList ?? Array.Empty<PlayerId>();
            this.CardList = CardList ?? Array.Empty<Card>();
            this.CardDefList = CardDefList ?? Array.Empty<CardDef>();
        }
    }
}
