using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared;
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

    private PlayerId PlayerId;
    private GameContext currentContext;

    private Action<string> LogInfo { get; set; }
    private Action<string> LogError { get; set; }

    public CardId[] MyCreatureCards => this.currentContext
        .You.PublicPlayerInfo.Field
        .Where(card => card.Type == CardType.Creature)
        .Select(c => c.Id)
        .ToArray();

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

    public async UniTask<GameId> OpenNewGame()
    {
        this.LogInfo("OpenNewGame: " + this.PlayerName);

        var ruleBook = new RuleBook(
            InitialMp: 1,
            MaxLimitMp: 10,
            MinMp: 0,
            LimitMpToIncrease: 1,
            InitialNumHands: 5,
            MaxNumHands: 10,
            InitialPlayerHp: 10,
            MaxPlayerHp: 10,
            MinPlayerHp: 0,
            MaxNumDeckCards: 40,
            MinNumDeckCards: 10,
            MaxNumFieldCards: 5,
            DefaultNumTurnsToCanAttack: 1,
            DefaultNumAttacksLimitInTurn: 1
        );

        var reply = await this.magiconionClient.OpenNewGame(new OpenNewGameRequest(ruleBook));

        this.GameId = reply.GameId;

        return this.GameId;
    }

    public async UniTask<RuleBook> GetRuleBook()
    {
        return await this.magiconionClient.GetRuleBook();
    }

    public async UniTask<CardDef[]> GetCardPool()
    {
        return await this.magiconionClient.GetCardPool();
    }

    public UniTask<PlayerId> EnterGame(IDeck deck)
    {
        if (this.GameId == default)
        {
            throw new InvalidOperationException("selected invalid game");
        }

        return this.EnterGame(this.GameId, deck);
    }

    public async UniTask<PlayerId> EnterGame(GameId gameId, IDeck deck)
    {
        this.currentContext = null;
        this.GameId = gameId;

        this.LogInfo($"GetCardPool: {this.PlayerName}: {this.GameId}: {this.PlayerId}");

        var cardDefs = await this.magiconionClient.GetCardPoolByGame(gameId);

        this.LogInfo($"reponse GetCardPool: {this.PlayerName}: {this.GameId}: {this.PlayerId}");

        var deckCardIds = this.ToDeckIdList(deck, cardDefs).ToArray();

        this.LogInfo($"EnterGame: {this.PlayerName}: {this.GameId}: {this.PlayerId}");

        var reply = await this.magiconionClient.EnterGame(new EnterGameRequest(this.GameId, this.PlayerName, deckCardIds));

        this.LogInfo($"response EnterGame: {this.PlayerName}: {this.GameId}: {this.PlayerId}");

        this.PlayerId = reply.PlayerId;

        return this.PlayerId;
    }

    public async UniTask LeaveGame()
    {
        if (this.GameId == default)
        {
            return;
        }

        await this.magiconionClient.LeaveGame(this.GameId);
    }

    private IEnumerable<CardDefId> ToDeckIdList(IDeck deck, CardDef[] cardPool)
    {
        return deck.CardDefNames
            .Select(cardName => cardPool.FirstOrDefault(c => c.FullName == cardName))
            .Where(cardDef => cardDef != null && !cardDef.IsToken)
            .Select(cardDef => cardDef.Id);
    }

    public async UniTask ReadyGame()
    {
        this.LogInfo($"ReadyGame: {this.PlayerName}: {this.GameId}: {this.PlayerId}");
        await this.magiconionClient.ReadyGame(new ReadyGameRequest(this.GameId, this.PlayerId));
    }

    public async UniTask StartTurn()
    {
        await this.PlayAction(async () =>
        {
            var reply = await this.magiconionClient.StartTurn(new StartTurnRequest(this.GameId, this.PlayerId));

            this.currentContext = reply.GameContext;
        });
    }

    public async UniTask PlayFromHand(CardId cardId)
    {
        await this.PlayAction(async () =>
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

    public async UniTask<(GameMasterStatusCode, CardId[])> ListPlayableCardId()
    {
        return await this.magiconionClient.ListPlayableCardId(this.GameId);
    }

    public async UniTask Attack(CardId attackCardId, CardId guardCardId)
    {
        await this.magiconionClient.AttackToCreature(new AttackToCreatureRequest(this.GameId, this.PlayerId, attackCardId, guardCardId));
    }

    public async UniTask<(PlayerId[], CardId[])> ListAttackTargets(CardId cardId)
    {
        var (status, (pList, cList)) = await this.magiconionClient.ListAttackTargets(this.GameId, cardId);

        if (status != GameMasterStatusCode.OK)
        {
            this.LogError(status.ToString());
            return (Array.Empty<PlayerId>(), Array.Empty<CardId>());
        }

        return (pList, cList);
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

    public async UniTask<GameOutline[]> ListOpenGames()
    {
        return await this.magiconionClient.ListOpenGames();
    }
}
