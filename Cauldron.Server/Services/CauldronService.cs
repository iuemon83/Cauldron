using Cauldron.Grpc.Api;
using Cauldron.Server.Models;
using Cauldron.Server.Models.Effect;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Server
{
    public class CauldronService : Cauldron.Grpc.Api.Cauldron.CauldronBase
    {
        private static Grpc.Models.GameContext CreateGameContext(GameId gameId, PlayerId PlayerId)
        {
            var gameMaster = new GameMasterRepository().GetById(gameId);

            var gameContext = gameMaster.CreateGameContext(PlayerId);

            return new Grpc.Models.GameContext()
            {
                GameOver = gameContext.GameOver,
                //RuleBook
                WinnerPlayerId = gameContext.WinnerPlayerId.ToString(),
                You = new Grpc.Models.PrivatePlayerInfo(gameContext.You),
                Opponent = new Grpc.Models.PublicPlayerInfo(gameContext.Opponent),
                RuleBook = new Grpc.Models.RuleBook(gameContext.RuleBook)
            };
        }

        private static void NotifyClient(PlayerId playerId, ReadyGameReply clientNotify)
        {
            if (!queueByGameId.TryGetValue(playerId, out var messageQueue))
            {
                throw new ArgumentException($"指定されたゲームが存在しない: {playerId}");
            }

            messageQueue.Enqueue(clientNotify);
        }

        private static GameMasterStatusCode IsPlayable(GameId gameId, PlayerId playerId)
        {
            var gameMaster = gameMasterRepository.GetById(gameId);

            if (!gameMaster.IsStarted)
            {
                return GameMasterStatusCode.NotStart;
            }

            if (gameMaster.GameOver)
            {
                return GameMasterStatusCode.GameOver;
            }

            if (playerId != gameMaster.ActivePlayer.Id)
            {
                return GameMasterStatusCode.NotActivePlayer;
            }

            return GameMasterStatusCode.OK;
        }

        private static readonly ConcurrentDictionary<PlayerId, ConcurrentQueue<ReadyGameReply>> queueByGameId = new();

        private static readonly GameMasterRepository gameMasterRepository = new();

        private readonly ILogger<CauldronService> _logger;

        public CauldronService(ILogger<CauldronService> logger)
        {
            _logger = logger;
        }

        private ChoiceResult AskCard(PlayerId playerId, ChoiceResult choiceResult, int numPicks)
        {
            var pickedPlayers = choiceResult.PlayerList.Take(numPicks).ToArray();
            numPicks -= pickedPlayers.Length;

            var pickedCards = choiceResult.CardList.Take(numPicks).ToArray();
            //numPicks -= pickedCards.Length;

            //var pickedCarddefs = choiceResult.CardDefList.Take(numPicks).ToArray();

            return new ChoiceResult()
            {
                PlayerList = pickedPlayers,
                CardList = pickedCards,
                //CardDefList = pickedCarddefs
            };
        }

        public override Task<OpenNewGameReply> OpenNewGame(OpenNewGameRequest request, ServerCallContext context)
        {
            var ruleBook = new RuleBook(request.RuleBook);
            var cardFactory = new CardFactory(ruleBook);
            cardFactory.SetCardPool(new CardPool().Load());

            var options = new GameMasterOptions(ruleBook, cardFactory, this._logger, this.AskCard, NotifyClient);
            var gameId = new GameMasterRepository().Add(options);

            return Task.FromResult(new OpenNewGameReply
            {
                GameId = gameId.ToString(),
            });
        }

        public override Task<GetCardPoolReply> GetCardPool(GetCardPoolRequest request, ServerCallContext context)
        {
            var gameId = GameId.Parse(request.GameId);
            var gameMaster = new GameMasterRepository().GetById(gameId);
            var cards = gameMaster.CardPool
            //var cards = new Core.CardPool().Load()
                .Select(cardDef => new Cauldron.Grpc.Models.CardDef(cardDef));

            return Task.FromResult(new GetCardPoolReply
            {
                Cards = { cards },
            });
        }

        public override Task<CloseGameReply> CloseGame(CloseGameRequest request, ServerCallContext context)
        {
            var gameId = GameId.Parse(request.GameId);
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

        public override Task<EnterGameReply> EnterGame(EnterGameRequest request, ServerCallContext context)
        {
            var gameId = GameId.Parse(request.GameId);

            var deckIds = request.DeckCardIds.Select(deckId => CardDefId.Parse(deckId));

            var gameMaster = new GameMasterRepository().GetById(gameId);

            var (status, newPlayerId) = gameMaster.CreateNewPlayer(request.PlayerName, deckIds);
            return status switch
            {
                GameMasterStatusCode.OK => Task.FromResult(new EnterGameReply()
                {
                    PlayerId = newPlayerId.ToString()
                }),
                GameMasterStatusCode.IsIncludedTokensInDeck => throw new RpcException(new Status(StatusCode.InvalidArgument, "deck include token")),
                _ => throw new RpcException(new Status(StatusCode.Unknown, "unknown error")),
            };
        }

        public override async Task ReadyGame(ReadyGameRequest request, IServerStreamWriter<ReadyGameReply> responseStream, ServerCallContext context)
        {
            var gameId = GameId.Parse(request.GameId);
            var playerId = PlayerId.Parse(request.PlayerId);

            var queue = new ConcurrentQueue<ReadyGameReply>();
            queueByGameId.TryAdd(playerId, queue);

            this._logger.LogInformation("二人揃ってない");

            await responseStream.WriteAsync(new ReadyGameReply()
            {
                Code = ReadyGameReply.Types.Code.Ready
            });

            Matching.Default.Ready(gameId, playerId, firstReadyPlayerId =>
            {
                this._logger.LogInformation("二人揃った");

                if (playerId == firstReadyPlayerId)
                {
                    var gameMaster = new GameMasterRepository().GetById(gameId);
                    gameMaster.Start(firstReadyPlayerId);
                }

                responseStream.WriteAsync(new ReadyGameReply()
                {
                    Code = ReadyGameReply.Types.Code.StartGame
                });
            });

            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (queue.TryDequeue(out var r))
                {
                    // メッセージがある
                    r.GameContext = CreateGameContext(gameId, playerId);
                    await responseStream.WriteAsync(r);
                }
            }

            this._logger.LogInformation("切れた");
        }

        public override Task<StartTurnReply> StartTurn(StartTurnRequest request, ServerCallContext context)
        {
            var gameId = GameId.Parse(request.GameId);
            var playerId = PlayerId.Parse(request.PlayerId);

            var playableStatus = IsPlayable(gameId, playerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                return Task.FromResult(new StartTurnReply()
                {
                    Result = playableStatus == GameMasterStatusCode.OK,
                    ErrorMessage = playableStatus.ToString(),
                    GameContext = CreateGameContext(gameId, playerId)
                });
            }

            var gameMaster = new GameMasterRepository().GetById(gameId);
            var statusCode = gameMaster.StartTurn();

            return Task.FromResult(new StartTurnReply()
            {
                Result = statusCode == GameMasterStatusCode.OK,
                ErrorMessage = statusCode.ToString(),
                GameContext = CreateGameContext(gameId, playerId)
            });
        }

        public override Task<EndTurnReply> EndTurn(EndTurnRequest request, ServerCallContext context)
        {
            var gameId = GameId.Parse(request.GameId);
            var playerId = PlayerId.Parse(request.PlayerId);

            var playableStatus = IsPlayable(gameId, playerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                return Task.FromResult(new EndTurnReply()
                {
                    Result = playableStatus == GameMasterStatusCode.OK,
                    ErrorMessage = playableStatus.ToString(),
                    GameContext = CreateGameContext(gameId, playerId)
                });
            }

            var gameMaster = new GameMasterRepository().GetById(gameId);
            var statusCode = gameMaster.EndTurn();

            return Task.FromResult(new EndTurnReply()
            {
                Result = statusCode == GameMasterStatusCode.OK,
                ErrorMessage = statusCode.ToString(),
                GameContext = CreateGameContext(gameId, playerId)
            });
        }

        public override Task<PlayFromHandReply> PlayFromHand(PlayFromHandRequest request, ServerCallContext context)
        {
            var gameId = GameId.Parse(request.GameId);
            var playerId = PlayerId.Parse(request.PlayerId);

            var playableStatus = IsPlayable(gameId, playerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                return Task.FromResult(new PlayFromHandReply()
                {
                    Result = playableStatus == GameMasterStatusCode.OK,
                    ErrorMessage = playableStatus.ToString(),
                    GameContext = CreateGameContext(gameId, playerId)
                });
            }

            var gameMaster = new GameMasterRepository().GetById(gameId);
            var handCardId = CardId.Parse(request.HandCardId);
            var statusCode = gameMaster.PlayFromHand(playerId, handCardId);

            return Task.FromResult(new PlayFromHandReply()
            {
                Result = statusCode == GameMasterStatusCode.OK,
                ErrorMessage = statusCode.ToString(),
                GameContext = CreateGameContext(gameId, playerId)
            });
        }

        public override Task<AttackToCreatureReply> AttackToCreature(AttackToCreatureRequest request, ServerCallContext context)
        {
            var gameId = GameId.Parse(request.GameId);
            var playerId = PlayerId.Parse(request.PlayerId);

            var playableStatus = IsPlayable(gameId, playerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                return Task.FromResult(new AttackToCreatureReply()
                {
                    Result = playableStatus == GameMasterStatusCode.OK,
                    ErrorMessage = playableStatus.ToString(),
                    GameContext = CreateGameContext(gameId, playerId)
                });
            }

            var gameMaster = new GameMasterRepository().GetById(gameId);
            var attackHandCardId = CardId.Parse(request.AttackHandCardId);
            var guardHandCardId = CardId.Parse(request.GuardHandCardId);
            var statusCode = gameMaster.AttackToCreature(playerId, attackHandCardId, guardHandCardId);

            return Task.FromResult(new AttackToCreatureReply()
            {
                Result = statusCode == GameMasterStatusCode.OK,
                ErrorMessage = statusCode.ToString(),
                GameContext = CreateGameContext(gameId, playerId)
            });
        }

        public override Task<AttackToPlayerReply> AttackToPlayer(AttackToPlayerRequest request, ServerCallContext context)
        {
            var gameId = GameId.Parse(request.GameId);
            var playerId = PlayerId.Parse(request.PlayerId);

            var playableStatus = IsPlayable(gameId, playerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                return Task.FromResult(new AttackToPlayerReply()
                {
                    Result = playableStatus == GameMasterStatusCode.OK,
                    ErrorMessage = playableStatus.ToString(),
                    GameContext = CreateGameContext(gameId, playerId)
                });
            }

            var gameMaster = new GameMasterRepository().GetById(gameId);
            var attackHandCardId = CardId.Parse(request.AttackHandCardId);
            var guardPlayerId = PlayerId.Parse(request.GuardPlayerId);
            var statusCode = gameMaster.AttackToPlayer(playerId, attackHandCardId, guardPlayerId);

            return Task.FromResult(new AttackToPlayerReply()
            {
                Result = statusCode == GameMasterStatusCode.OK,
                ErrorMessage = statusCode.ToString(),
                GameContext = CreateGameContext(gameId, playerId)
            });
        }
    }
}
