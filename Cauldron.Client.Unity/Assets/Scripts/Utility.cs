using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    static class Utility
    {
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
                    ? gameContext.You.Hands.FirstOrDefault(c => c.Id == cardId)
                    : null,
                ZoneName.Field => zonePlayer.Field.FirstOrDefault(c => c.Id == cardId),
                ZoneName.Cemetery => zonePlayer.Cemetery.FirstOrDefault(c => c.Id == cardId),
                _ => null
            };

            if (card == null) return ("", "");

            var ownerName = GetPlayerName(gameContext, card.OwnerId);

            return (ownerName, card.Name);
        }

        public static (string ownerPlayerName, string cardName) GetCardName(GameContext gameContext, CardId cardId)
        {
            // 非公開領域に移動した場合は取れない
            var card = gameContext.You.Hands
                .Concat(gameContext.You.PublicPlayerInfo.Field)
                .Concat(gameContext.You.PublicPlayerInfo.Cemetery)
                .Concat(gameContext.Opponent.Field)
                .Concat(gameContext.Opponent.Cemetery)
                .FirstOrDefault(c => c.Id == cardId);

            if (card == default)
            {
                return ("", "");
            }

            var ownerName = GetPlayerName(gameContext, card.OwnerId);

            return (ownerName, card.Name);
        }

        public static void LoadAsyncScene(MonoBehaviour monoBehaviour, SceneNames sceneName, Action onLoadAction = null)
        {
            monoBehaviour.StartCoroutine(LoadAsyncSceneCoroutine(sceneName, onLoadAction));
        }

        private static IEnumerator LoadAsyncSceneCoroutine(SceneNames sceneName, Action onLoadAction)
        {
            yield return SceneManager.LoadSceneAsync(sceneName.ToString(), LoadSceneMode.Additive);

            onLoadAction?.Invoke();

            yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
