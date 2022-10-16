using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Core;
using Cauldron.Core.Entities;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Server.Services
{
    public class CauldronHub : StreamingHubBase<ICauldronHub, ICauldronHubReceiver>, ICauldronHub
    {
        public readonly static int GiveUpAnswerTimeInSeconds = 300;

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

        private SqliteConnection dbConnection;

        public CauldronHub(IConfiguration configuration, ILogger<CauldronHub> logger)
        {
            this.configuration = configuration;
            this._logger = logger;
        }

        protected override async ValueTask OnConnecting()
        {
            // handle connection if needed.
            this._logger.LogInformation("client connected {context_id}", this.Context.ContextId);

            this.dbConnection = new BattleLogDb().Connection();
            await this.dbConnection.OpenAsync();
        }

        protected override async ValueTask OnDisconnected()
        {
            // handle disconnection if needed.
            // on disconnecting, if automatically removed this connection from group.

            this._logger.LogInformation("on disconnected {name}", this.self?.Name ?? "");

            await (this as ICauldronHub).LeaveRoom();

            if (this.dbConnection != null)
            {
                await this.dbConnection.DisposeAsync();
            }

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

            this._logger.LogInformation("called {name}: questionId={currentQuestionId}, choiceCandidates={choiceCandidates}", nameof(AskCard), currentQuestionId, choiceCandidates);
            this.BroadcastTo(this.room, playerId.Value).OnAsk(new AskMessage(
                currentQuestionId,
                choiceCandidates,
                numPicks));

            var answer = await Task.Run(async () =>
            {
                var s = new Stopwatch();

                try
                {
                    s.Start();

                    ChoiceAnswer answer;
                    while (!QuestionManager.TryGetAnswer(currentQuestionId, out answer))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(0.2));

                        //TODO エラー飛ばすけどクライアント側で処理してない
                        // エラーになると、このカードのPlayFromHandが失敗する。（クライアント側が選択して送ってきても何も起きない）
                        // その後に、なにかアクションすると再接続して、処理が続行される
                        if (s.Elapsed >= TimeSpan.FromSeconds(GiveUpAnswerTimeInSeconds))
                        {
                            throw new InvalidOperationException("give up recieve answer");
                        }
                    }

                    return answer;
                }
                finally
                {
                    s.Stop();
                }
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
        Task<ListAllowedClientVersionsReply> ICauldronHub.ListAllowedClientVersions()
        {
            return Task.FromResult(new ListAllowedClientVersionsReply(this.configuration["AllowedClientVersions"].Split(",")));
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<RoomOutline[]> ICauldronHub.ListOpenGames()
        {
            return Task.FromResult(gameMasterRepository.ListOpenRooms());
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<GameReplay[]> ICauldronHub.ListGameHistories(ListGameHistoriesRequest request)
        {
            return Task.FromResult(
                new BattleLogDb().ListGameHistories(
                    this.dbConnection,
                    request.GameIdList,
                    request.ClientId,
                    request.OnlyMyLogs
                    )
                .ToArray()
                );
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<CardDef[]> ICauldronHub.GetCardPoolByGame(GameId gameId)
        {
            return Task.FromResult(
                new BattleLogDb().FindCardPool(this.dbConnection, gameId)
                );
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<int> ICauldronHub.FirstActionLog(GameId gameId)
        {
            var (success, room) = await this.Group.TryAddAsync(
                Guid.NewGuid().ToString(),
                1, true);

            if (!success)
            {
                // 失敗
                throw new InvalidOperationException("failed create room!!!!");
            }

            this.room = room;
            return await ((ICauldronHub)this).NextActionLog(gameId, new PlayerId(Guid.Empty), -1);
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        Task<int> ICauldronHub.NextActionLog(GameId gameId, PlayerId playerId, int currentActionLogId)
        {
            if (this.room == null)
            {
                throw new InvalidOperationException("room is null");
            }

            this._logger.LogInformation("current action id={currentActionLogId}", currentActionLogId);

            var log = new BattleLogDb().FindNextActionLog(this.dbConnection, gameId, playerId, currentActionLogId);
            if (log == null)
            {
                this._logger.LogInformation("end replay!!!!!!!!!");
                return Task.FromResult(-1);
            }

            this._logger.LogInformation("next action id={actionLogId}", log.ActionLogId);

            switch (log.NotifyEvent)
            {
                case NotifyEvent.OnPlay:
                    {
                        var message = JsonConverter.Deserialize<PlayCardNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnPlayCard(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnStartTurn:
                    {
                        var message = JsonConverter.Deserialize<StartTurnNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnStartTurn(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnAddCard:
                    {
                        var message = JsonConverter.Deserialize<AddCardNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnAddCard(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnExcludeCard:
                    {
                        var message = JsonConverter.Deserialize<ExcludeCardNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnExcludeCard(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnMoveCard:
                    {
                        var message = JsonConverter.Deserialize<MoveCardNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnMoveCard(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnModifyCard:
                    {
                        var message = JsonConverter.Deserialize<ModifyCardNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnModifyCard(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnModifyPlayer:
                    {
                        var message = JsonConverter.Deserialize<ModifyPlayerNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnModifyPlayer(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnBattleStart:
                    {
                        var message = JsonConverter.Deserialize<BattleNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnBattleStart(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnBattleEnd:
                    {
                        var message = JsonConverter.Deserialize<BattleNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnBattleEnd(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnDamage:
                    {
                        var message = JsonConverter.Deserialize<DamageNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnDamage(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnHeal:
                    {
                        var message = JsonConverter.Deserialize<HealNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnHeal(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnModityCounter:
                    {
                        var message = JsonConverter.Deserialize<ModifyCounterNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnModifyCounter(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnModifyNumFields:
                    {
                        var message = JsonConverter.Deserialize<ModifyNumFieldsNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnModifyNumFields(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.OnEndGame:
                    {
                        var message = JsonConverter.Deserialize<EndGameNotifyMessage>(log.MessageJson);
                        this.Broadcast(this.room).OnEndGame(log.GameContext, message);
                        break;
                    }
                case NotifyEvent.AskCardAction:
                    {
                        //var message = JsonConverter.Deserialize<EndGameNotifyMessage>(log.MessageJson);
                        //this.Broadcast(this.room).OnAsk(log.GameContext, message);
                        break;
                    }
            }

            return Task.FromResult(log.ActionLogId);
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<OpenNewRoomReply> ICauldronHub.OpenNewRoom(OpenNewRoomRequest request)
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

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsStartTurnEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnPlay: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnPlayCard(gameContext, message);
                        this._logger.LogInformation("OnPlayCard: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsPlayEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnAddCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnAddCard(gameContext, message);
                        this._logger.LogInformation("OnAddCard: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsAddCardEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnExcludeCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnExcludeCard(gameContext, message);
                        this._logger.LogInformation("OnExcludeCard: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsExcludeCardEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnBattleStart: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnBattleStart(gameContext, message);
                        this._logger.LogInformation("OnBattle: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsBattleStartEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnBattleEnd: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnBattleEnd(gameContext, message);
                        this._logger.LogInformation("OnBattle: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsBattleEndEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnDamage: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnDamage(gameContext, message);
                        this._logger.LogInformation("OnDamage: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsDamageEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnHeal: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnHeal(gameContext, message);
                        this._logger.LogInformation("OnHeal: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsHealEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnModifyCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnModifyCard(gameContext, message);
                        this._logger.LogInformation("OnModifyCard: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsModifyCardEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnModifyPlayer: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnModifyPlayer(gameContext, message);
                        this._logger.LogInformation("OnModifyPlayer: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsModifyPlayerEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnMoveCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnMoveCard(gameContext, message);
                        this._logger.LogInformation("OnMoveCard: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsMoveCardEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnModityCounter: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnModifyCounter(gameContext, message);
                        this._logger.LogInformation("OnModifyCounter: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsModityCounterEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnModifyNumFields: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnModifyNumFields(gameContext, message);
                        this._logger.LogInformation("OnModifyNumFields: {playerId}", playerId);

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsModityNumFieldsEvent(this.gameId, playerId, gameContext, message));
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    OnEndGame: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnEndGame(gameContext, message);
                        this._logger.LogInformation($"OnEndGame:");

                        try
                        {
                            new BattleLogDb().Add(this.dbConnection,
                                BattleLog.AsEndGameEvent(this.gameId, playerId, gameContext, message));

                            new BattleLogDb().SetGameWinner(this.dbConnection,
                                this.gameId, gameContext.WinnerPlayerId);
                        }
                        catch (Exception e)
                        {
                            this._logger.LogError(e, "db error");
                        }
                    },
                    AskCardAction: this.AskCard
                ));

            if (this.gameId != default)
            {
                await (this as ICauldronHub).LeaveRoom();
            }

            this.gameId = gameMasterRepository.Add(request.OwnerName, request.Message, options);

            var (found, gameMaster) = gameMasterRepository.TryGetById(this.gameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }

            try
            {
                var enterResult = await this.JoinRoom(this.gameId, request.OwnerName, request.DeckCardIdList, gameMaster);

                switch (enterResult.StatusCode)
                {
                    case JoinRoomReply.StatusCodeValue.InvalidDeck:
                        return new OpenNewRoomReply(OpenNewRoomReply.StatusCodeValue.InvalidDeck, default, default);

                    case JoinRoomReply.StatusCodeValue.RoomIsFull:
                        throw new Exception("room is full");
                }

                return new OpenNewRoomReply(OpenNewRoomReply.StatusCodeValue.Ok, this.gameId, enterResult.PlayerId);
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "system error");
                await (this as ICauldronHub).LeaveRoom();

                throw;
            }
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<bool> ICauldronHub.LeaveRoom()
        {
            this._logger.LogInformation("leave game {name}", this.self?.Name ?? "");

            if (this.room == null)
            {
                return true;
            }

            if (this.gameId == default)
            {
                return true;
            }

            var roomRemoved = await this.room.RemoveAsync(this.Context);

            if (roomRemoved)
            {
                gameMasterRepository.Delete(this.gameId);
                numOfReadiesByGameId.TryRemove(this.gameId, out var _);
            }

            this.room = null;
            this.gameId = default;

            return true;
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<JoinRoomReply> ICauldronHub.JoinRoom(JoinRoomRequest request)
        {
            var (found, gameMaster) = gameMasterRepository.TryGetById(request.GameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }

            return await this.JoinRoom(request.GameId, request.PlayerName, request.DeckCardIdList, gameMaster);
        }

        private async Task<JoinRoomReply> JoinRoom(GameId gameId, string playerName, CardDefId[] deckCardIdList, GameMaster gameMaster)
        {
            var (status, newPlayerId) = gameMaster.CreateNewPlayer(
                new PlayerId(this.ConnectionId),
                playerName,
                deckCardIdList
                );

            switch (status)
            {
                case GameMasterStatusCode.OK:
                    // グループに参加したり
                    var success = false;
                    try
                    {
                        this.self = gameMaster.PlayerDefsById[newPlayerId];
                        (success, room, storage) = await this.Group.TryAddAsync(
                            gameId.ToString(), 2, true, this.self);

                    }
                    catch (Exception e)
                    {
                        this._logger.LogInformation("{a}", e.ToString());
                    }

                    if (success)
                    {
                        this.gameId = gameId;
                        this.BroadcastExceptSelf(this.room).OnJoinGame();
                        return new JoinRoomReply(newPlayerId, JoinRoomReply.StatusCodeValue.Ok);
                    }
                    else
                    {
                        //TODO 追加したプレイヤーを削除
                        return new JoinRoomReply(newPlayerId, JoinRoomReply.StatusCodeValue.RoomIsFull);
                    }

                case GameMasterStatusCode.InvalidDeck:
                    return new JoinRoomReply(newPlayerId, JoinRoomReply.StatusCodeValue.InvalidDeck);

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

            var (found, gameMaster) = gameMasterRepository.TryGetById(request.GameId);
            if (!found)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "invalid game id"));
            }

            try
            {
                new BattleLogDb().Add(this.dbConnection,
                    new GameLog(this.gameId, gameMaster.CardPool));

                await gameMaster.StartGame(firstPlayerId);

                var logs = gameMaster.playerRepository.AllPlayers
                    .Select(p =>
                    {
                        var playOrder = firstPlayerId == p.Id ? 1 : 2;
                        var cardNamesInDeck = p.Hands.AllCards.Concat(p.Deck.AllCards).Select(c => c.Name).ToArray();
                        //var ip = this.Context.CallContext.GetHttpContext().Connection.RemoteIpAddress?.ToString() ?? "";

                        return new BattlePlayer(
                            request.GameId, p.Id, request.ClientId,
                            p.Name, cardNamesInDeck, playOrder, ""
                            );
                    });

                foreach (var log in logs)
                {
                    try
                    {
                        new BattleLogDb().Add(this.dbConnection, log);
                    }
                    catch (Exception e)
                    {
                        this._logger.LogError(e, "db error");
                    }
                }
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "db error");
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
