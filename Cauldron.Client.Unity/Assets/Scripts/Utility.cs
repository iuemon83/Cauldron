using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    static class Utility
    {
        public static GameId? GameId = null;

        public static T RandomPick<T>(IReadOnlyList<T> source) => source.Any() ? source[UnityEngine.Random.Range(0, source.Count)] : default;

        public static string GetPlayerName(GameContext gameContext, PlayerId playerId)
        {
            return gameContext.You.PublicPlayerInfo.Id == playerId
                ? gameContext.You.PublicPlayerInfo.Name
                : gameContext.Opponent.Name;
        }

        public static (string ownerPlayerName, string cardName) GetCardName(GameContext gameContext, Zone zone, CardId cardId)
        {
            var zonePlayer = gameContext.You.PublicPlayerInfo.Id == zone.PlayerId
                ? gameContext.You.PublicPlayerInfo
                : gameContext.Opponent;

            var card = zone.ZoneName switch
            {
                ZoneName.Hand => gameContext.You.PublicPlayerInfo.Id == zone.PlayerId
                    ? gameContext.You.Hands.First(c => c.Id == cardId)
                    : null,
                ZoneName.Field => zonePlayer.Field.First(c => c.Id == cardId),
                ZoneName.Cemetery => zonePlayer.Cemetery.First(c => c.Id == cardId),
                _ => null
            };

            if (card == null) return ("", "");

            var ownerName = GetPlayerName(gameContext, card.OwnerId);

            return (ownerName, card.Name);
        }

        public static (string ownerPlayerName, string cardName) GetCardName(GameContext gameContext, CardId cardId)
        {
            var card = gameContext.You.Hands
                .Concat(gameContext.You.PublicPlayerInfo.Field)
                .Concat(gameContext.You.PublicPlayerInfo.Cemetery)
                .Concat(gameContext.Opponent.Field)
                .Concat(gameContext.Opponent.Cemetery)
                .First(c => c.Id == cardId);

            var ownerName = GetPlayerName(gameContext, card.OwnerId);

            return (ownerName, card.Name);
        }
    }
}
