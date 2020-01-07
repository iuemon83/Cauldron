namespace Cauldron.Server.Models
{
    public class PrivatePlayerInfo : PublicPlayerInfo
    {
        public Hands Hands { get; }

        public PrivatePlayerInfo(Player player)
            : base(player)
        {
            this.Hands = player.Hands;
        }

        public PrivatePlayerInfo(PublicPlayerInfo publicPlayerInfo, Hands hands)
            : base(publicPlayerInfo.Id, publicPlayerInfo.Name, publicPlayerInfo.Field, publicPlayerInfo.DeckCount, publicPlayerInfo.Hp, publicPlayerInfo.MaxMp, publicPlayerInfo.UsedMp)
        {
            this.Hands = hands;
        }
    }
}
