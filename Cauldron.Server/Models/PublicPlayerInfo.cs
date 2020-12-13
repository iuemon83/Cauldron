using System;

namespace Cauldron.Server.Models
{
    public record PublicPlayerInfo(
        Guid Id,
        string Name,
        Field Field,
        int DeckCount,
        int HandsCount,
        int Hp,
        int MaxMp,
        int UsedMp,
        int UsableMp
        )
    {
        public PublicPlayerInfo(Player player)
            : this(
                 player.Id,
                 player.Name,
                 player.Field,
                 player.Deck.Count,
                 player.Hands.Count,
                 player.CurrentHp,
                 player.MaxMp,
                 player.UsedMp,
                 player.CurrentMp
                 )
        { }
    }
}
