using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Linq;

public class Client
{
    public static async UniTask<Client> Factory(string serverAddress, string playerName,
        ICauldronHubReceiver cauldronHubReceiver,
        Action<string> logInfo, Action<string> logError)
    {
        return await Client.Factory(new Grpc.Core.Channel(serverAddress, Grpc.Core.ChannelCredentials.Insecure),
             playerName, default, cauldronHubReceiver, logInfo, logError);
    }

    public static async UniTask<Client> Factory(Grpc.Core.Channel channel, string playerName,
        ICauldronHubReceiver cauldronHubReceiver,
        Action<string> logInfo, Action<string> logError)
    {
        return await Client.Factory(channel, playerName, default, cauldronHubReceiver, logInfo, logError);
    }

    public static async UniTask<Client> Factory(Grpc.Core.Channel channel,
        string playerName, GameId gameId, ICauldronHubReceiver cauldronHubReceiver,
        Action<string> logInfo, Action<string> logError)
    {
        var client = new Client(channel, playerName, gameId, logInfo, logError);
        await client.Connect(cauldronHubReceiver);

        return client;
    }

    private readonly Grpc.Core.Channel channel;
    private ICauldronHub magiconionClient;
    private ICauldronService magiconionServiceClient;

    public GameId GameId { get; private set; }
    public readonly string PlayerName;

    public PlayerId PlayerId { get; private set; }
    private GameContext currentContext;

    private Action<string> LogInfo { get; set; }
    private Action<string> LogError { get; set; }

    public CardId[] MyCreatureCards => this.currentContext
        .You.PublicPlayerInfo.Field
        .OfType<Card>()
        .Where(card => card.Type == CardType.Creature)
        .Select(c => c.Id)
        .ToArray();

    public CardId[] PlayableCardIdList => this.currentContext.You.PlayableCards;

    private Client(Grpc.Core.Channel channel, string playerName, GameId gameId,
        Action<string> logInfo, Action<string> logError)
    {
        this.PlayerName = playerName;
        this.GameId = gameId;

        this.channel = channel;
        this.LogInfo = logInfo;
        this.LogError = logError;
    }

    private async UniTask Connect(ICauldronHubReceiver cauldronHubReceiver)
    {
        var logger = new ClientLogger();
        this.magiconionServiceClient = MagicOnionClient.Create<ICauldronService>(this.channel);
        this.magiconionClient = await StreamingHubClient.ConnectAsync<ICauldronHub, ICauldronHubReceiver>(
            this.channel, cauldronHubReceiver, logger: logger);
    }

    public async UniTask Destroy()
    {
        this.LogInfo("destroy " + this.PlayerName);

        await this.magiconionClient.DisposeAsync();
        await this.channel.ShutdownAsync();
    }

    private async UniTask<bool> PlayAction(Func<UniTask> action)
    {
        if (this.currentContext?.GameOver ?? false)
        {
            var winner = this.currentContext.WinnerPlayerId == this.PlayerId
                ? this.currentContext.You.PublicPlayerInfo.Name
                : this.currentContext.Opponent.Name;
            this.LogInfo($"{winner} の勝ち！");
            return false;
        }

        await action();

        return true;
    }

    public async UniTask<OpenNewRoomReply> OpenNewRoom(string message, IDeck deck)
    {
        this.LogInfo("OpenNewGame: " + this.PlayerName);

        this.currentContext = null;

        var cardDefs = await this.magiconionClient.GetCardPool();

        var ruleBook = await this.GetRuleBook();
        var deckCardIds = this.ToDeckIdList(deck, cardDefs).ToArray();

        var reply = await this.magiconionClient.OpenNewRoom(
            new OpenNewRoomRequest(ruleBook, this.PlayerName, message, deckCardIds));

        this.GameId = reply.GameId;
        this.PlayerId = reply.PlayerId;

        return reply;
    }

    public async UniTask<RuleBook> GetRuleBook()
    {
        return await this.magiconionClient.GetRuleBook();
    }

    public async UniTask<CardDef[]> GetCardPool()
    {
        return await this.magiconionClient.GetCardPool();
    }

    public async UniTask<ListAllowedClientVersionsReply> ListAllowedClientVersions()
    {
        return await this.magiconionClient.ListAllowedClientVersions();
    }

    public UniTask<JoinRoomReply> JoinRoom(IDeck deck)
    {
        if (this.GameId == default)
        {
            throw new InvalidOperationException("selected invalid game");
        }

        return this.JoinRoom(this.GameId, deck);
    }

    public async UniTask<JoinRoomReply> JoinRoom(GameId gameId, IDeck deck)
    {
        this.currentContext = null;
        this.GameId = gameId;

        this.LogInfo($"GetCardPool: {this.PlayerName}: {this.GameId}: {this.PlayerId}");

        var cardDefs = await this.magiconionClient.GetCardPool();

        this.LogInfo($"reponse GetCardPool: {this.PlayerName}: {this.GameId}: {this.PlayerId}");

        var deckCardIds = this.ToDeckIdList(deck, cardDefs).ToArray();

        this.LogInfo($"EnterGame: {this.PlayerName}: {this.GameId}: {this.PlayerId}");

        var reply = await this.magiconionClient.JoinRoom(new JoinRoomRequest(this.GameId, this.PlayerName, deckCardIds));

        this.LogInfo($"response EnterGame: {this.PlayerName}: {this.GameId}: {this.PlayerId}");

        this.PlayerId = reply.PlayerId;

        return reply;
    }

    public async UniTask LeaveRoom()
    {
        if (this.GameId == default)
        {
            return;
        }

        await this.magiconionClient.LeaveRoom();
    }

    private IEnumerable<CardDefId> ToDeckIdList(IDeck deck, CardDef[] cardPool)
    {
        return deck.CardDefNames
            .Select(cardName => cardPool.FirstOrDefault(c => c.FullName == cardName))
            .Select(cardDef => cardDef?.Id ?? default);
    }

    public async UniTask ReadyGame()
    {
        this.LogInfo($"ReadyGame: {this.PlayerName}: {this.GameId}: {this.PlayerId}");
        await this.magiconionClient.ReadyGame(
            new ReadyGameRequest(this.GameId, this.PlayerId, LocalData.ClientId)
            );
    }

    public async UniTask StartTurn()
    {
        await this.PlayAction(async () =>
        {
            var reply = await this.magiconionClient.StartTurn(new StartTurnRequest(this.GameId, this.PlayerId));

            this.currentContext = reply.GameContext;
        });
    }

    public async UniTask<bool> PlayFromHand(CardId cardId)
    {
        return await this.PlayAction(async () =>
        {
            var reply = await this.magiconionClient.PlayFromHand(new PlayFromHandRequest(this.GameId, this.PlayerId, cardId));

            if (!reply.Result)
            {
                this.LogError(reply.ErrorMessage);
                return;
            }

            this.currentContext = reply.GameContext;
        });
    }

    public async UniTask Attack(CardId attackCardId, CardId guardCardId)
    {
        await this.magiconionClient.AttackToCreature(new AttackToCreatureRequest(this.GameId, this.PlayerId, attackCardId, guardCardId));
    }

    public AttackTarget ListAttackTargets(CardId cardId)
    {
        if (this.currentContext == null)
        {
            return new AttackTarget(Array.Empty<PlayerId>(), Array.Empty<CardId>());
        }

        return this.currentContext.You.PublicPlayerInfo.AttackableCardIdList.TryGetValue(cardId, out var targets)
            ? targets
            : new AttackTarget(Array.Empty<PlayerId>(), Array.Empty<CardId>());
    }

    public async UniTask AttackToOpponentPlayer(CardId attackCardId)
    {
        await this.magiconionClient.AttackToPlayer(new AttackToPlayerRequest(this.GameId, this.PlayerId, attackCardId, this.currentContext.Opponent.Id));
    }

    public async UniTask EndTurn()
    {
        await this.PlayAction(async () =>
        {
            var reply = await this.magiconionClient.EndTurn(new EndTurnRequest(this.GameId, this.PlayerId));
            this.currentContext = reply.GameContext;
        });
    }

    public async UniTask<GameMasterStatusCode> Surrender()
    {
        return await this.magiconionClient.Surrender(this.GameId);
    }

    public async UniTask<GameMasterStatusCode> AnswerChoice(Guid questionId, ChoiceAnswer choiceAnswer)
    {
        this.LogInfo($"answer: questionId={questionId}");

        var result = await this.magiconionServiceClient.AnswerChoice(questionId, choiceAnswer);

        return result;
    }

    public async UniTask<RoomOutline[]> ListOpenGames()
    {
        return await this.magiconionClient.ListOpenGames();
    }

    public async UniTask<GameReplay[]> ListGameHistories(ListGameHistoriesRequest request)
    {
        return await this.magiconionClient.ListGameHistories(request);
    }

    public async UniTask<int> FirstActionLog(GameId gameId)
    {
        return await this.magiconionClient.FirstActionLog(gameId);
    }

    public async UniTask<int> NextActionLog(GameId gameId, PlayerId playerId, int currentActionLogId)
    {
        return await this.magiconionClient.NextActionLog(gameId, playerId, currentActionLogId);
    }
}
