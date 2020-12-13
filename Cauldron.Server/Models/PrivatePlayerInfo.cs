namespace Cauldron.Server.Models
{
    public record PrivatePlayerInfo(PublicPlayerInfo PublicPlayerInfo, Hands Hands)
    {
        public PrivatePlayerInfo(Player player)
            : this(
            new PublicPlayerInfo(player),
            player.Hands
            )
        { }
    }
}
