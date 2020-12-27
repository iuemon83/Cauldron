using Cauldron.Server.Models;
using System;
using System.Collections.Concurrent;

namespace Cauldron.Server
{
    public class Matching
    {
        public static readonly Matching Default = new Matching();

        private readonly ConcurrentDictionary<GameId, (PlayerId FirstReadyPlayerId, Action<PlayerId> Callback)> readyFirstPlayerListByGameId = new();

        public void Ready(GameId gameId, PlayerId playerId, Action<PlayerId> startGameCallback)
        {
            if (this.readyFirstPlayerListByGameId.TryAdd(gameId, (playerId, startGameCallback)))
            {
                // 一人目ならなにもしない
                return;
            }

            var (FirstReadyPlayerId, Callback) = this.readyFirstPlayerListByGameId[gameId];

            // 二人目ならゲーム開始
            if (playerId != FirstReadyPlayerId)
            {
                Callback(FirstReadyPlayerId);
                startGameCallback(FirstReadyPlayerId);
            }
        }
    }
}
