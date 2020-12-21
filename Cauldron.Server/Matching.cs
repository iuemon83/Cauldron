using System;
using System.Collections.Concurrent;

namespace Cauldron.Server
{
    public class Matching
    {
        public static readonly Matching Default = new Matching();

        private readonly ConcurrentDictionary<Guid, (Guid FirstReadyPlayerId, Action<Guid> Callback)> readyFirstPlayerListByGameId = new();

        public void Ready(Guid gameId, Guid playerId, Action<Guid> startGameCallback)
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
