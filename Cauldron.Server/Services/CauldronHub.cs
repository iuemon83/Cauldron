using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Server.Services
{
    public class CauldronHub : StreamingHubBase<ICauldronHub, ICauldronHubReceiver>, ICauldronHub
    {
        private static GameContext CreateGameContext(GameId gameId, PlayerId PlayerId)
        {
            var (found, gameMaster) = gameMasterRepository.TryGetById(gameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }

            return gameMaster.CreateGameContext(PlayerId);
        }

        private static GameMasterStatusCode IsPlayable(GameId gameId, PlayerId playerId)
        {
            var (found, gameMaster) = gameMasterRepository.TryGetById(gameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }

            if (!gameMaster.IsGameStarted)
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

        private static readonly GameMasterRepository gameMasterRepository = new();

        private static readonly ConcurrentDictionary<GameId, int> numOfReadiesByGameId = new();

        private readonly IConfiguration configuration;
        private readonly ILogger<CauldronHub> _logger;

        private IGroup room;
        private PlayerDef self;
        private GameId gameId;
        private IInMemoryStorage<PlayerDef> storage;

        public CauldronHub(IConfiguration configuration, ILogger<CauldronHub> logger)
        {
            this.configuration = configuration;
            this._logger = logger;
        }

        protected override async ValueTask OnDisconnected()
        {
            await (this as ICauldronHub).LeaveGame(this.gameId);

            await CompletedTask;
        }

        private async ValueTask<ChoiceAnswer> AskCard(PlayerId playerId, ChoiceCandidates choiceCandidates, int numPicks)
        {
            var currentQuestionId = QuestionManager.AddNewQuestion();

            if (choiceCandidates.Count == 0)
            {
                return new ChoiceAnswer(
                    Array.Empty<PlayerId>(),
                    Array.Empty<CardId>(),
                    Array.Empty<CardDefId>()
                    );
            }

            try
            {
                this._logger.LogInformation("called {name}: questionId={currentQuestionId}, choiceCandidates={choiceCandidates}", nameof(AskCard), currentQuestionId, choiceCandidates);
                this.BroadcastTo(this.room, playerId.Value).OnAsk(new AskMessage(
                    currentQuestionId,
                    choiceCandidates,
                    numPicks));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var answer = await Task.Run(() =>
            {
                ChoiceAnswer answer;
                while (!QuestionManager.TryGetAnswer(currentQuestionId, out answer)) ;

                return answer;
            });

            // 答えが来た

            return answer;
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<CardDef[]> ICauldronHub.GetCardPool()
        {
            var cardRepository = new CardRepository(RuleBookInitializer.RuleBookSingleton);
            cardRepository.SetCardPool(CardPoolInitializer.CardPoolSingleton);

            return Task.FromResult(cardRepository.CardPool.ToArray());
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<RuleBook> ICauldronHub.GetRuleBook()
        {
            return Task.FromResult(RuleBookInitializer.RuleBookSingleton);
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<GameOutline[]> ICauldronHub.ListOpenGames()
        {
            return Task.FromResult(gameMasterRepository.ListOpenGames());
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<OpenNewGameReply> ICauldronHub.OpenNewGame(OpenNewGameRequest request)
        {
            var ruleBook = request.RuleBook;
            var cardRepository = new CardRepository(ruleBook);
            cardRepository.SetCardPool(CardPoolInitializer.CardPoolSingleton);

            var options = new GameMasterOptions(ruleBook, cardRepository, this._logger,
                new GameEventListener(
                    OnStartTurn: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnStartTurn(gameContext, message);
                        this._logger.LogInformation("OnStartTurn: {playerId}", playerId);
                    },
                    OnPlay: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnPlayCard(gameContext, message);
                        this._logger.LogInformation("OnPlayCard: {playerId}", playerId);
                    },
                    OnAddCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnAddCard(gameContext, message);
                        this._logger.LogInformation("OnAddCard: {playerId}", playerId);
                    },
                    OnExcludeCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnExcludeCard(gameContext, message);
                        this._logger.LogInformation("OnExcludeCard: {playerId}", playerId);
                    },
                    OnBattleStart: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnBattleStart(gameContext, message);
                        this._logger.LogInformation("OnBattle: {playerId}", playerId);
                    },
                    OnBattleEnd: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnBattleEnd(gameContext, message);
                        this._logger.LogInformation("OnBattle: {playerId}", playerId);
                    },
                    OnDamage: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnDamage(gameContext, message);
                        this._logger.LogInformation("OnDamage: {playerId}", playerId);
                    },
                    OnModifyCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnModifyCard(gameContext, message);
                        this._logger.LogInformation("OnModifyCard: {playerId}", playerId);
                    },
                    OnModifyPlayer: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnModifyPlayer(gameContext, message);
                        this._logger.LogInformation("OnModifyPlayer: {playerId}", playerId);
                    },
                    OnMoveCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnMoveCard(gameContext, message);
                        this._logger.LogInformation("OnMoveCard: {playerId}", playerId);
                    },
                    OnModityCounter: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnModifyCounter(gameContext, message);
                        this._logger.LogInformation("OnModifyCounter: {playerId}", playerId);
                    },
                    OnEndGame: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnEndGame(gameContext, message);
                        this._logger.LogInformation($"OnEndGame:");
                    },
                    AskCardAction: this.AskCard
                ));

            this.gameId = gameMasterRepository.Add(options);

            return Task.FromResult(new OpenNewGameReply(this.gameId));
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<bool> ICauldronHub.LeaveGame(GameId gameId)
        {
            if (this.room == null)
            {
                return true;
            }

            var removed = await this.room.RemoveAsync(this.Context);
            if (!removed)
            {
                return false;
            }

            var numPlayers = await this.room.GetMemberCountAsync();
            if (numPlayers == 0)
            {
                gameMasterRepository.Delete(gameId);
            }

            this.room = null;

            return true;
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<CardDef[]> ICauldronHub.GetCardPoolByGame(GameId gameId)
        {
            var (found, gameMaster) = gameMasterRepository.TryGetById(gameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }

            var cards = gameMaster.CardPool.ToArray();

            this._logger.LogInformation("response {length}: {ConnectionId}", cards.Length, this.ConnectionId);

            return Task.FromResult(cards);
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<EnterGameReply> ICauldronHub.EnterGame(EnterGameRequest request)
        {
            var (found, gameMaster) = gameMasterRepository.TryGetById(request.GameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }

            var (status, newPlayerId) = gameMaster.CreateNewPlayer(new PlayerId(this.ConnectionId), request.PlayerName, request.DeckCardIdList);

            switch (status)
            {
                case GameMasterStatusCode.OK:
                    // グループに参加したり
                    var success = false;
                    try
                    {
                        this.self = gameMaster.PlayerDefsById[newPlayerId];
                        (success, room, storage) = await this.Group.TryAddAsync(request.GameId.ToString(), 2, true, this.self);

                    }
                    catch (Exception e)
                    {
                        this._logger.LogInformation("{a}", e.ToString());
                    }

                    if (success)
                    {
                        this.BroadcastExceptSelf(this.room).OnJoinGame();
                    }
                    else
                    {
                        //TODO 追加したプレイヤーを削除
                        throw new RpcException(new Status(StatusCode.InvalidArgument, "room is full"));
                    }

                    return new EnterGameReply(newPlayerId);

                case GameMasterStatusCode.InvalidDeck:
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid deck"));

                default:
                    throw new RpcException(new Status(StatusCode.Unknown, "unknown error"));
            }
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task ICauldronHub.ReadyGame(ReadyGameRequest request)
        {
            var numOfReadies = numOfReadiesByGameId.AddOrUpdate(request.GameId, 1, (_, num) => num + 1);

            this.BroadcastToSelf(this.room).OnReady();

            var numOfPlayers = await this.room.GetMemberCountAsync();

            var readyAll = numOfPlayers == 2
                && numOfReadies == 2;

            if (!readyAll)
            {
                // ぜんいんが準備完了しない限りはなにもしない
                this._logger.LogInformation("二人揃ってない");
                return;
            }

            this._logger.LogInformation("二人揃った");

            // ふたりとも準備完了なら開始
            this.Broadcast(this.room).OnStartGame();

            // 先行プレイヤーはランダムで選択する
            var firstPlayerId = this.storage.AllValues.OrderBy(_ => Guid.NewGuid()).First().Id;

            try
            {
                var (found, gameMaster) = gameMasterRepository.TryGetById(request.GameId);
                if (!found)
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
                }
                await gameMaster.StartGame(firstPlayerId);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<StartTurnReply> ICauldronHub.StartTurn(StartTurnRequest request)
        {
            var playableStatus = IsPlayable(request.GameId, request.PlayerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={playableStatus}", playableStatus);

                return new StartTurnReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var (found, gameMaster) = gameMasterRepository.TryGetById(request.GameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }
            var statusCode = await gameMaster.StartTurn();

            if (statusCode != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={statusCode}", statusCode);
            }

            return new StartTurnReply(
                statusCode == GameMasterStatusCode.OK,
                statusCode.ToString(),
                CreateGameContext(request.GameId, request.PlayerId)
            );
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<EndTurnReply> ICauldronHub.EndTurn(EndTurnRequest request)
        {
            var playableStatus = IsPlayable(request.GameId, request.PlayerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={playableStatus}", playableStatus);

                return new EndTurnReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var (found, gameMaster) = gameMasterRepository.TryGetById(request.GameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }
            var statusCode = await gameMaster.EndTurn();

            if (statusCode != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={statusCode}", statusCode);
            }

            return new EndTurnReply(
                statusCode == GameMasterStatusCode.OK,
                statusCode.ToString(),
                CreateGameContext(request.GameId, request.PlayerId)
            );
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<PlayFromHandReply> ICauldronHub.PlayFromHand(PlayFromHandRequest request)
        {
            var playableStatus = IsPlayable(request.GameId, request.PlayerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={playableStatus}", playableStatus);

                return new PlayFromHandReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var (found, gameMaster) = gameMasterRepository.TryGetById(request.GameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }
            var statusCode = await gameMaster.PlayFromHand(request.PlayerId, request.HandCardId);

            if (statusCode != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={statusCode}", statusCode);
            }

            return new PlayFromHandReply(
                statusCode == GameMasterStatusCode.OK,
                statusCode.ToString(),
                CreateGameContext(request.GameId, request.PlayerId)
            );
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<AttackToCreatureReply> ICauldronHub.AttackToCreature(AttackToCreatureRequest request)
        {
            var playableStatus = IsPlayable(request.GameId, request.PlayerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={playableStatus}", playableStatus);

                return new AttackToCreatureReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var (found, gameMaster) = gameMasterRepository.TryGetById(request.GameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }
            var statusCode = await gameMaster.AttackToCreature(request.PlayerId, request.AttackCardId, request.GuardCardId);

            if (statusCode != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={statusCode}", statusCode);
            }

            return new AttackToCreatureReply(
                statusCode == GameMasterStatusCode.OK,
                statusCode.ToString(),
                CreateGameContext(request.GameId, request.PlayerId)
            );
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<AttackToPlayerReply> ICauldronHub.AttackToPlayer(AttackToPlayerRequest request)
        {
            var playableStatus = IsPlayable(request.GameId, request.PlayerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={playableStatus}", playableStatus);

                return new AttackToPlayerReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var (found, gameMaster) = gameMasterRepository.TryGetById(request.GameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }
            var statusCode = await gameMaster.AttackToPlayer(request.PlayerId, request.AttackCardId, request.GuardPlayerId);

            if (statusCode != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={statusCode}", statusCode);
            }

            return new AttackToPlayerReply(
                statusCode == GameMasterStatusCode.OK,
                statusCode.ToString(),
                CreateGameContext(request.GameId, request.PlayerId)
            );
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<(GameMasterStatusCode, (PlayerId[], CardId[]))> ICauldronHub.ListAttackTargets(GameId gameId, CardId cardId)
        {
            var playableStatus = IsPlayable(gameId, this.self.Id);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                this._logger.LogWarning("result={playableStatus}", playableStatus);
                return Task.FromResult((playableStatus, default((PlayerId[], CardId[]))));
            }

            var (found, gameMaster) = gameMasterRepository.TryGetById(gameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }

            var attackTargetsResult = gameMaster.ListAttackTargets(cardId);

            return Task.FromResult(attackTargetsResult);
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<GameMasterStatusCode> ICauldronHub.Surrender(GameId gameId)
        {
            var (found, gameMaster) = gameMasterRepository.TryGetById(gameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }

            return Task.FromResult(gameMaster.Surrender(this.self.Id));
        }
    }
}
