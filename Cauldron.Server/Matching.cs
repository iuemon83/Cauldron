using System;
using System.Collections.Generic;

namespace Cauldron.Server
{
    public class Matching
    {
        public static Matching Default = new Matching();

        private readonly Dictionary<Guid, Guid> readPlayerIdListByGameId = new Dictionary<Guid, Guid>();

        public void Ready(Guid gameId, Guid playerId, Action<Guid> startGameCallback)
        {
            if (!this.readPlayerIdListByGameId.TryGetValue(gameId, out var firstReadyPlayerId))
            {
                this.readPlayerIdListByGameId.Add(gameId, playerId);
                return;
            }

            if (playerId != firstReadyPlayerId)
            {
                startGameCallback(firstReadyPlayerId);
            }
        }
    }
}
