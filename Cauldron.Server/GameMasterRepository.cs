using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Cauldron.Server
{
    public class GameMasterRepository
    {
        public static readonly ConcurrentDictionary<GameId, GameMaster> gameMasterListByGameId = new();

        public GameId Add(Grpc.Models.RuleBook ruleBook, CardFactory cardFactory, ILogger logger,
            Func<PlayerId, ChoiceResult, int, ChoiceResult> askCardAction,
            Action<PlayerId, Grpc.Api.ReadyGameReply> notifyClientAction)
        {
            var id = GameId.NewId();
            var gameMaster = new GameMaster(new RuleBook(ruleBook), cardFactory, logger, askCardAction, notifyClientAction);
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
