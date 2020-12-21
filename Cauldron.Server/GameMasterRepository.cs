using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Cauldron.Server
{
    public class GameMasterRepository
    {
        public static readonly ConcurrentDictionary<Guid, GameMaster> gameMasterListByGameId = new();

        public Guid Add(Grpc.Models.RuleBook ruleBook, CardFactory cardFactory, ILogger logger,
            Func<Guid, ChoiceResult, int, ChoiceResult> askCardAction,
            Action<Guid, Grpc.Api.ReadyGameReply> notifyClientAction)
        {
            var id = Guid.NewGuid();
            var gameMaster = new GameMaster(new RuleBook(ruleBook), cardFactory, logger, askCardAction, notifyClientAction);
            gameMasterListByGameId.TryAdd(id, gameMaster);

            return id;
        }

        public void Delete(Guid gameId)
        {
            gameMasterListByGameId.TryRemove(gameId, out _);
        }

        public GameMaster GetById(Guid gameId)
        {
            return gameMasterListByGameId[gameId];
        }
    }
}
