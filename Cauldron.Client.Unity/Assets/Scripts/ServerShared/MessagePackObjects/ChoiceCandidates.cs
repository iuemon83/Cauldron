using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ChoiceCandidates
    {
        public PlayerId[] PlayerIdList { get; } = Array.Empty<PlayerId>();
        public Card[] CardList { get; } = Array.Empty<Card>();
        public CardDef[] CardDefList { get; } = Array.Empty<CardDef>();

        public ChoiceCandidates(PlayerId[] PlayerIdList, Card[] CardList, CardDef[] CardDefList)
        {
            this.PlayerIdList = PlayerIdList;
            this.CardList = CardList;
            this.CardDefList = CardDefList;
        }
    }
}
