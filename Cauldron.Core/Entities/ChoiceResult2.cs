using System;

namespace Cauldron.Shared.MessagePackObjects
{
    public class ChoiceResult2
    {
        public PlayerId[] PlayerIdList { get; } = Array.Empty<PlayerId>();
        public Card[] CardList { get; } = Array.Empty<Card>();
        public CardDef[] CardDefList { get; } = Array.Empty<CardDef>();

        public ChoiceResult2(PlayerId[] PlayerIdList, Card[] CardList, CardDef[] CardDefList)
        {
            this.PlayerIdList = PlayerIdList;
            this.CardList = CardList;
            this.CardDefList = CardDefList;
        }
    }
}
