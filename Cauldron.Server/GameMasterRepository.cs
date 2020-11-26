using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Cauldron.Server
{
    public class GameMasterRepository
    {
        public static readonly Dictionary<Guid, GameMaster> gameMasterListByGameId = new Dictionary<Guid, GameMaster>();

        public Guid Add(Cauldron.Grpc.Models.RuleBook ruleBook, CardFactory cardFactory, ILogger logger,
            Func<Guid, ChoiceResult, int, ChoiceResult> askCardAction)
        {
            var id = Guid.NewGuid();
            var cruleBook = new RuleBook()
            {
                InitialMp = ruleBook.InitialMp,
                InitialNumHands = ruleBook.InitialNumHands,
                InitialPlayerHp = ruleBook.InitialPlayerHp,
                MaxMp = ruleBook.MaxMp,
                MaxNumDeckCards = ruleBook.MaxNumDeckCards,
                MaxNumFieldCars = ruleBook.MaxNumFieldCars,
                MaxNumHands = ruleBook.MaxNumHands,
                MaxPlayerHp = ruleBook.MaxPlayerHp,
                MinMp = ruleBook.MinMp,
                MinNumDeckCards = ruleBook.MinNumDeckCards,
                MinPlayerHp = ruleBook.MinPlayerHp,
                MpByStep = ruleBook.MpByStep
            };
            var gameMaster = new GameMaster(cruleBook, cardFactory, logger, askCardAction);
            gameMasterListByGameId.Add(id, gameMaster);

            return id;
        }

        public void Delete(Guid gameId)
        {
            gameMasterListByGameId.Remove(gameId);
        }

        public GameMaster GetById(Guid gameId)
        {
            return gameMasterListByGameId[gameId];
        }
    }
}
