using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        public static void LoadAsyncScene(MonoBehaviour monoBehaviour, SceneNames sceneName)
        {
            monoBehaviour.StartCoroutine(LoadAsyncSceneCoroutine(sceneName));
        }

        private static IEnumerator LoadAsyncSceneCoroutine(SceneNames sceneName)
        {

            // The Application loads the Scene in the background as the current Scene runs.
            // This is particularly good for creating loading screens.
            // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
            // a sceneBuildIndex of 1 as shown in Build Settings.

            var asyncLoad = SceneManager.LoadSceneAsync(sceneName.ToString());

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
    }
}
