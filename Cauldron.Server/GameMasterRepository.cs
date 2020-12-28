using Cauldron.Server.Models;
using System.Collections.Concurrent;

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

        public GameMaster GetById(GameId gameId)
        {
            return gameMasterListByGameId[gameId];
        }
    }
}
