namespace Cauldron.Core
{
    public class PrivatePlayerInfo : PublicPlayerInfo
    {
        public Hands Hands { get; }

        public PrivatePlayerInfo(Player player)
            : base(player)
        {
            this.Hands = player.Hands;
        }
    }
}
