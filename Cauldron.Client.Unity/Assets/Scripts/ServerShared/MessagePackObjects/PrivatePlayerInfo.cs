using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PrivatePlayerInfo
    {
        public PublicPlayerInfo PublicPlayerInfo { get; }
        public Card[] Hands { get; }
        public CardId[] PlayableCards { get; }

        public PrivatePlayerInfo(
            PublicPlayerInfo PublicPlayerInfo,
            Card[] Hands,
            CardId[] PlayableCards
            )
        {
            this.PublicPlayerInfo = PublicPlayerInfo;
            this.Hands = Hands;
            this.PlayableCards = PlayableCards;
        }
    }
}
