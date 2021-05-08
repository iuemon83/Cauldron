using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Core.Entities;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Concurrent;
using System.Linq;

namespace Cauldron.Server
{
    public class GameMasterRepository
    {
        public static readonly ConcurrentDictionary<GameId, GameMaster> gameMasterListByGameId = new();

        public GameId Add(GameMasterOptions options)
        {
            var id = GameId.NewId();
            var gameMaster = new GameMaster(options);
            gameMasterListByGameId.TryAdd(id, gameMaster);

            return id;
        }

        public void Delete(GameId gameId)
        {
            gameMasterListByGameId.TryRemove(gameId, out _);
        }

        public (bool, GameMaster) TryGetById(GameId gameId)
        {
            return gameMasterListByGameId.TryGetValue(gameId, out var value)
                ? (true, value)
                : (false, default);
        }

        public GameOutline[] ListOpenGames()
        {
            return gameMasterListByGameId
                .Where(gameMaster => gameMaster.Value.PlayerDefsById.Count == 1)
                .Select(gameMaster =>
                {
                    return new GameOutline(gameMaster.Key, gameMaster.Value.PlayerDefsById.First().Value.Name);
                })
                .ToArray();
        }
    }
}
