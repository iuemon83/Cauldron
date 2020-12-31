namespace Cauldron.Server.Models
{
    public record PublicPlayerInfo(
        PlayerId Id,
        string Name,
        Field Field,
        int DeckCount,
        Cemetery Cemetery,
        int HandsCount,
        int MaxHp,
        int CurrentHp,
        int MaxMp,
        int CurrentMp
        )
    {
        public PublicPlayerInfo(Player player)
            : this(
                 player.Id,
                 player.Name,
                 player.Field,
                 player.Deck.Count,
                 player.Cemetery,
                 player.Hands.AllCards.Count,
                 player.MaxHp,
                 player.CurrentHp,
                 player.MaxMp,
                 player.CurrentMp
                 )
        { }
    }
}
