using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Core.Entities;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Concurrent;
using System.Linq;

namespace Cauldron.Server
{
    public class GameMasterRepository
    {
        public static readonly ConcurrentDictionary<GameId, (GameMaster, RoomOutline)> gameMasterListByGameId = new();

        public GameId Add(string ownerName, string message, GameMasterOptions options)
        {
            var id = GameId.NewId();
            var gameMaster = new GameMaster(options);
            gameMasterListByGameId.TryAdd(id, (gameMaster, new RoomOutline(id, ownerName, message)));

            return id;
        }

        public void Delete(GameId gameId)
        {
            gameMasterListByGameId.TryRemove(gameId, out _);
        }

        public (bool, GameMaster) TryGetById(GameId gameId)
        {
            return gameMasterListByGameId.TryGetValue(gameId, out var value)
                ? (true, value.Item1)
                : (false, default);
        }

        public RoomOutline[] ListOpenRooms()
        {
            return gameMasterListByGameId.Values
                .Where(x => x.Item1.PlayerDefsById.Count == 1)
                .Select(x => x.Item2)
                .ToArray();
        }
    }
}
