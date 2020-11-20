using Cauldron.Grpc.Api;
using Cauldron.Grpc.Models;
using Cauldron.Server.Models;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Server
{
    public class CauldronService : Cauldron.Grpc.Api.Cauldron.CauldronBase
    {
        private readonly ILogger<CauldronService> _logger;
        public CauldronService(ILogger<CauldronService> logger)
        {
            _logger = logger;
        }

        public override Task<OpenNewGameReply> OpenNewGame(OpenNewGameRequest request, ServerCallContext context)
        {
            var cardFactory = new CardFactory();
            cardFactory.SetCardPool(new CardPool().Load());

            var gameId = new GameMasterRepository()
                .Add(request.RuleBook, cardFactory, this._logger, this.AskCard);

            //var deckIds = request.Deck
            //    .Select(deckId => Guid.Parse(deckId));

            //var gameMaster = new GameMasterRepository().GetById(gameId);
            //var newPlayerId = gameMaster.CreateNewPlayer(request.PlayerName, deckIds);

            return Task.FromResult(new OpenNewGameReply
            {
                GameId = gameId.ToString(),
                //PlayerId = newPlayerId.ToString(),
            });
        }

        public override Task<GetCardPoolReply> GetCardPool(GetCardPoolRequest request, ServerCallContext context)
        {
            var gameId = Guid.Parse(request.GameId);
            var gameMaster = new GameMasterRepository().GetById(gameId);
            var cards = gameMaster.CardPool
            //var cards = new Core.CardPool().Load()
                .Select(cardDef => new Cauldron.Grpc.Models.CardDef(cardDef));

            return Task.FromResult(new GetCardPoolReply
            {
                Cards = { cards },
            });
        }

        private Guid AskCard(Guid playerId, IReadOnlyList<Guid> candidates)
        {
            return candidates[0];
        }

        public override Task<CloseGameReply> CloseGame(CloseGameRequest request, ServerCallContext context)
        {
            var gameId = Guid.Parse(request.GameId);
            new GameMasterRepository().Delete(gameId);

            return Task.FromResult(new CloseGameReply
            {
                Result = true
            });
        }

        public override Task<SetDeckReply> SetDeck(SetDeckRequest request, ServerCallContext context)
        {
            return base.SetDeck(request, context);
        }

        public override Task ReadyGame(ReadyGameRequest request, IServerStreamWriter<ReadyGameReply> responseStream, ServerCallContext context)
        {
            var gameId = Guid.Parse(request.GameId);
            var playerId = Guid.Parse(request.PlayerId);

            Matching.Default.Ready(gameId, playerId, firstReadyPlayerId =>
            {
                var gameMaster = new GameMasterRepository().GetById(gameId);
                gameMaster.Start(firstReadyPlayerId);

                responseStream.WriteAsync(new ReadyGameReply()
                {
                    IsStart = true
                });
            });

            return responseStream.WriteAsync(new ReadyGameReply()
            {
                IsStart = false,
            });
        }

        public override Task<EnterGameReply> EnterGame(EnterGameRequest request, ServerCallContext context)
        {
            var gameId = Guid.Parse(request.GameId);

            var deckIds = request.DeckCardIds.Select(deckId => Guid.Parse(deckId));

            var gameMaster = new GameMasterRepository().GetById(gameId);
            var newPlayerId = gameMaster.CreateNewPlayer(request.PlayerName, deckIds);

            return Task.FromResult(new EnterGameReply()
            {
                PlayerId = newPlayerId.ToString()
            });
        }

        private GameContext CreateGameContext(Guid gameId, Guid PlayerId)
        {
            var gameMaster = new GameMasterRepository().GetById(gameId);

            var gameContext = gameMaster.CreateEnvironment(PlayerId);

            return new GameContext()
            {
                GameOver = gameContext.GameOver,
                //RuleBook
                WinnerPlayerId = gameContext.WinnerPlayerId.ToString(),
                You = new Grpc.Models.PrivatePlayerInfo(gameContext.You),
                Opponent = new Grpc.Models.PublicPlayerInfo(gameContext.Opponent),
                RuleBook = new Grpc.Models.RuleBook(gameContext.RuleBook)
            };
        }

        public override Task<StartTurnReply> StartTurn(StartTurnRequest request, ServerCallContext context)
        {
            var gameId = Guid.Parse(request.GameId);
            var gameMaster = new GameMasterRepository().GetById(gameId);

            var playerId = Guid.Parse(request.PlayerId);
            var (isSucceeded, errorMessage) = gameMaster.StartTurn(playerId);

            return Task.FromResult(new StartTurnReply()
            {
                Result = isSucceeded,
                ErrorMessage = errorMessage,
                GameContext = CreateGameContext(gameId, playerId)
            });
        }

        public override Task<EndTurnReply> EndTurn(EndTurnRequest request, ServerCallContext context)
        {
            var gameId = Guid.Parse(request.GameId);
            var gameMaster = new GameMasterRepository().GetById(gameId);

            var playerId = Guid.Parse(request.PlayerId);
            var (isSucceeded, errorMessage) = gameMaster.EndTurn(playerId);

            return Task.FromResult(new EndTurnReply()
            {
                Result = isSucceeded,
                ErrorMessage = errorMessage,
                GameContext = CreateGameContext(gameId, playerId)
            });
        }

        public override Task<PlayFromHandReply> PlayFromHand(PlayFromHandRequest request, ServerCallContext context)
        {
            var gameId = Guid.Parse(request.GameId);
            var gameMaster = new GameMasterRepository().GetById(gameId);

            var playerId = Guid.Parse(request.PlayerId);
            var handCardId = Guid.Parse(request.HandCardId);
            var (isSucceeded, errorMessage) = gameMaster.PlayFromHand(playerId, handCardId);

            return Task.FromResult(new PlayFromHandReply()
            {
                Result = isSucceeded,
                ErrorMessage = errorMessage,
                GameContext = CreateGameContext(gameId, playerId)
            });
        }

        public override Task<AttackToCreatureReply> AttackToCreature(AttackToCreatureRequest request, ServerCallContext context)
        {
            var gameId = Guid.Parse(request.GameId);
            var gameMaster = new GameMasterRepository().GetById(gameId);

            var playerId = Guid.Parse(request.PlayerId);
            var attackHandCardId = Guid.Parse(request.AttackHandCardId);
            var guardHandCardId = Guid.Parse(request.GuardHandCardId);
            var (isSucceeded, errorMessage) = gameMaster.AttackToCreature(playerId, attackHandCardId, guardHandCardId);

            return Task.FromResult(new AttackToCreatureReply()
            {
                Result = isSucceeded,
                ErrorMessage = errorMessage,
                GameContext = CreateGameContext(gameId, playerId)
            });
        }

        public override Task<AttackToPlayerReply> AttackToPlayer(AttackToPlayerRequest request, ServerCallContext context)
        {
            var gameId = Guid.Parse(request.GameId);
            var gameMaster = new GameMasterRepository().GetById(gameId);

            var playerId = Guid.Parse(request.PlayerId);
            var attackHandCardId = Guid.Parse(request.AttackHandCardId);
            var guardPlayerId = Guid.Parse(request.GuardPlayerId);
            var (isSucceeded, errorMessage) = gameMaster.AttackToPlayer(playerId, attackHandCardId, guardPlayerId);

            return Task.FromResult(new AttackToPlayerReply()
            {
                Result = isSucceeded,
                ErrorMessage = errorMessage,
                GameContext = CreateGameContext(gameId, playerId)
            });
        }
    }
}
