using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PrivatePlayerInfo
    {
        public PublicPlayerInfo PublicPlayerInfo { get; }
        public Card[] Hands { get; }

        public PrivatePlayerInfo(PublicPlayerInfo PublicPlayerInfo, Card[] Hands)
        {
            this.PublicPlayerInfo = PublicPlayerInfo;
            this.Hands = Hands;
        }
    }
}
