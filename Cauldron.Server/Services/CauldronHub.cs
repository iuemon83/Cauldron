﻿using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using Grpc.Core;
using MagicOnion.Server;
using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Server.Services
{
    public class CauldronHub : StreamingHubBase<ICauldronHub, ICauldronHubReceiver>, ICauldronHub
    {
        private static GameContext CreateGameContext(GameId gameId, PlayerId PlayerId)
        {
            var gameMaster = new GameMasterRepository().GetById(gameId);

            return gameMaster.CreateGameContext(PlayerId);
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

        private static readonly GameMasterRepository gameMasterRepository = new();

        private readonly IConfiguration configuration;
        private readonly ILogger<CauldronHub> _logger;

        private string CardSetDirectoryPath => this.configuration["CardSetDirectoryPath"];

        private IGroup room;
        private PlayerDef self;
        private IInMemoryStorage<PlayerDef> storage;

        public CauldronHub(IConfiguration configuration, ILogger<CauldronHub> logger)
        {
            this.configuration = configuration;
            this._logger = logger;
        }

        private async ValueTask<ChoiceResult> AskCard(PlayerId playerId, ChoiceCandidates choiceCandidates, int numPicks)
        {
            var currentQuestionId = QuestionManager.AddNewQuestion();

            try
            {
                this._logger.LogInformation($"called {nameof(AskCard)}: questionId={currentQuestionId}, choiceCandidates={choiceCandidates}");
                this.BroadcastTo(this.room, playerId.Value).OnChoiceCards(new ChoiceCardsMessage
                {
                    QuestionId = currentQuestionId,
                    ChoiceCandidates = choiceCandidates,
                    NumPicks = numPicks
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            var answer = await Task.Run(() =>
            {
                ChoiceResult answer;
                while (!QuestionManager.TryGetAnswer(currentQuestionId, out answer)) ;

                return answer;
            });

            // 答えが来た

            return answer;
        }


        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<int> ICauldronHub.Test(int num)
        {
            return num * 2;
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<OpenNewGameReply> ICauldronHub.OpenNewGame(OpenNewGameRequest request)
        {
            var ruleBook = request.RuleBook;
            var cardRepository = new CardRepository(ruleBook);
            cardRepository.SetCardPool(CardPool.ReadFromDirectory(this.CardSetDirectoryPath));

            var options = new GameMasterOptions(ruleBook, cardRepository, this._logger,
                new GameEventListener(
                    OnStartTurn: (playerId, gameContext) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnStartTurn(gameContext);
                        this._logger.LogInformation($"OnStartTurn: {playerId}");
                    },
                    OnAddCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnAddCard(gameContext, message);
                        this._logger.LogInformation($"OnAddCard: {playerId}");
                    },
                    OnDamage: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnDamage(gameContext, message);
                        this._logger.LogInformation($"OnDamage: {playerId}");
                    },
                    OnModifyCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnModifyCard(gameContext, message);
                        this._logger.LogInformation($"OnModifyCard: {playerId}");
                    },
                    OnModifyPlayer: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnModifyPlayer(gameContext, message);
                        this._logger.LogInformation($"OnModifyPlayer: {playerId}");
                    },
                    OnMoveCard: (playerId, gameContext, message) =>
                    {
                        this.BroadcastTo(this.room, playerId.Value).OnMoveCard(gameContext, message);
                        this._logger.LogInformation($"OnMoveCard: {playerId}");
                    },
                    AskCardAction: this.AskCard
                ));

            var gameId = new GameMasterRepository().Add(options);

            return new OpenNewGameReply(gameId);
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<CloseGameReply> ICauldronHub.CloseGame(CloseGameRequest request)
        {
            new GameMasterRepository().Delete(request.GameId);

            return new CloseGameReply(true, "");
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<SetDeckReply> ICauldronHub.SetDeck(SetDeckRequest request)
        {
            return new SetDeckReply();
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<GetCardPoolReply> ICauldronHub.GetCardPool(GetCardPoolRequest request)
        {
            var gameMaster = new GameMasterRepository().GetById(request.GameId);

            var cards = gameMaster.CardPool.ToArray();

            this._logger.LogInformation($"response {cards.Length}: {this.ConnectionId}");

            return new GetCardPoolReply(cards);
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<EnterGameReply> ICauldronHub.EnterGame(EnterGameRequest request)
        {
            var gameMaster = new GameMasterRepository().GetById(request.GameId);

            var numOfPlayers = this.room == null
                ? 0
                : await this.room.GetMemberCountAsync();
            if (numOfPlayers == 2)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "room is full"));
            }

            var (status, newPlayerId) = gameMaster.CreateNewPlayer(new PlayerId(this.ConnectionId), request.PlayerName, request.DeckCardIdList);

            switch (status)
            {
                case GameMasterStatusCode.OK:
                    // グループに参加したり
                    try
                    {
                        this.self = gameMaster.PlayerDefsById[newPlayerId];
                        (room, storage) = await this.Group.AddAsync(request.GameId.ToString(), this.self);
                    }
                    catch (Exception e)
                    {
                        this._logger.LogInformation(e.ToString());
                    }
                    return new EnterGameReply(newPlayerId);

                case GameMasterStatusCode.IsIncludedTokensInDeck:
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "deck include token"));

                default:
                    throw new RpcException(new Status(StatusCode.Unknown, "unknown error"));
            }
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task ICauldronHub.ReadyGame(ReadyGameRequest request)
        {
            this.self.Ready = true;

            this.BroadcastToSelf(this.room).OnReady(new GameContext());

            var numOfPlayers = await this.room.GetMemberCountAsync();

            var readyAll = numOfPlayers == 2
                && this.storage.AllValues.All(p => p.Ready);

            if (!readyAll)
            {
                // ぜんいんが準備完了しない限りはなにもしない
                this._logger.LogInformation("二人揃ってない");
                return;
            }

            this._logger.LogInformation("二人揃った");

            // ふたりとも準備完了なら開始
            this.Broadcast(this.room).OnStartGame(new GameContext());

            if (this.storage.AllValues.First().Id == this.self.Id)
            {
                // 一度だけ実行したいから部屋主の接続で実行する

                var firstPlayerId = this.storage.AllValues.OrderBy(_ => Guid.NewGuid()).First().Id;

                var gameMaster = new GameMasterRepository().GetById(request.GameId);
                await gameMaster.Start(firstPlayerId);
            }
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<StartTurnReply> ICauldronHub.StartTurn(StartTurnRequest request)
        {
            var playableStatus = IsPlayable(request.GameId, request.PlayerId);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                return new StartTurnReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var gameMaster = new GameMasterRepository().GetById(request.GameId);
            var statusCode = await gameMaster.StartTurn();

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
                return new EndTurnReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var gameMaster = new GameMasterRepository().GetById(request.GameId);
            var statusCode = await gameMaster.EndTurn();

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
                return new PlayFromHandReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var gameMaster = new GameMasterRepository().GetById(request.GameId);
            var statusCode = await gameMaster.PlayFromHand(request.PlayerId, request.HandCardId);

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
                return new AttackToCreatureReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var gameMaster = new GameMasterRepository().GetById(request.GameId);
            var statusCode = await gameMaster.AttackToCreature(request.PlayerId, request.AttackCardId, request.GuardCardId);

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
                return new AttackToPlayerReply(
                    playableStatus == GameMasterStatusCode.OK,
                    playableStatus.ToString(),
                    CreateGameContext(request.GameId, request.PlayerId)
                );
            }

            var gameMaster = new GameMasterRepository().GetById(request.GameId);
            var statusCode = await gameMaster.AttackToPlayer(request.PlayerId, request.AttackCardId, request.GuardPlayerId);

            return new AttackToPlayerReply(
                statusCode == GameMasterStatusCode.OK,
                statusCode.ToString(),
                CreateGameContext(request.GameId, request.PlayerId)
            );
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<(GameMasterStatusCode, CardId[])> ICauldronHub.ListPlayableCardId(GameId gameId)
        {
            var playableStatus = IsPlayable(gameId, this.self.Id);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                return (playableStatus, default);
            }

            var gameMaster = new GameMasterRepository().GetById(gameId);

            var ListPlayableCardIdResult = gameMaster.ListPlayableCardId(this.self.Id);

            return ListPlayableCardIdResult;
        }

        [FromTypeFilter(typeof(LoggingAttribute))]
        async Task<(GameMasterStatusCode, (PlayerId[], CardId[]))> ICauldronHub.ListAttackTargets(GameId gameId, CardId cardId)
        {
            var playableStatus = IsPlayable(gameId, this.self.Id);
            if (playableStatus != GameMasterStatusCode.OK)
            {
                return (playableStatus, default);
            }

            var gameMaster = new GameMasterRepository().GetById(gameId);

            var attackTargetsResult = gameMaster.ListAttackTargets(cardId);

            return attackTargetsResult;
        }
    }
}
