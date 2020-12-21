using System;

namespace Cauldron.Server.Models
{
    public record PublicPlayerInfo(
        Guid Id,
        string Name,
        Field Field,
        int DeckCount,
        int CemeteryCount,
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
                 player.Cemetery.Count,
                 player.Hands.Count,
                 player.MaxHp,
                 player.CurrentHp,
                 player.MaxMp,
                 player.CurrentMp
                 )
        { }
    }
}
