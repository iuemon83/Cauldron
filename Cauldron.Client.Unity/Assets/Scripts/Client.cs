using Assets.Scripts;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using Grpc.Core;
using MagicOnion.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

public class Client
{
    private readonly Channel channel;
    private readonly ICauldronHub magiconionClient;
    private readonly ICauldronService magiconionServiceClient;
    private readonly string playerName;

    public GameId GameId { get; private set; }
    private PlayerId PlayerId;
    private GameContext currentContext;

    private Action<string> Logging { get; set; }
    private Action<string> LoggingError { get; set; }

    public Client(string playerName, ICauldronHubReceiver cauldronHubReceiver, Action<string> logInfo, Action<string> logError)
        : this(playerName, default, cauldronHubReceiver, logInfo, logError)
    {
    }

    public Client(string playerName, GameId gameId, ICauldronHubReceiver cauldronHubReceiver, Action<string> logInfo, Action<string> logError)
    {
        this.playerName = playerName;
        this.GameId = gameId;

        this.channel = new Channel("localhost:5000", ChannelCredentials.Insecure);
        var logger = new ClientLogger();
        this.magiconionServiceClient = MagicOnionClient.Create<ICauldronService>(channel);
        this.magiconionClient = StreamingHubClient.Connect<ICauldronHub, ICauldronHubReceiver>(channel, cauldronHubReceiver, logger: logger);
        this.Logging = logInfo;
        this.LoggingError = logError;
    }

    public async Task Destroy()
    {
        await this.magiconionClient.DisposeAsync();
        await this.channel.ShutdownAsync();
    }

    public async ValueTask<int> Test()
    {
        this.Logging("Test!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

        var result = await this.magiconionClient.Test(12);
        return result;
    }

    public async ValueTask<bool> PlayTurn()
    {
        return
            await this.PlayAction(() => this.StartTurn())
            && await this.PlayAction(() => this.PlayFromHand())
            && await this.PlayAction(() => this.Attack())
            && await this.PlayAction(() => this.EndTurn());
    }

    private async ValueTask<bool> PlayAction(Func<ValueTask> action)
    {
        if (this.currentContext?.GameOver ?? false)
        {
            var winner = this.currentContext.WinnerPlayerId == this.PlayerId
                ? this.currentContext.You.PublicPlayerInfo.Name
                : this.currentContext.Opponent.Name;
            this.Logging($"{winner} の勝ち！");
            return false;
        }

        await action();

        return true;
    }

    public async ValueTask<GameId> OpenNewGame()
    {
        this.Logging("OpenNewGame: " + this.playerName);

        var ruleBook = new RuleBook(
            InitialMp: 1,
            MaxLimitMp: 10,
            MinMp: 0,
            MpByStep: 1,
            InitialNumHands: 5,
            MaxNumHands: 10,
            InitialPlayerHp: 10,
            MaxPlayerHp: 10,
            MinPlayerHp: 0,
            MaxNumDeckCards: 40,
            MinNumDeckCards: 40,
            MaxNumFieldCars: 5,
            DefaultNumTurnsToCanAttack: 1,
            DefaultNumAttacksLimitInTurn: 1
        );

        var reply = await this.magiconionClient.OpenNewGame(new OpenNewGameRequest(ruleBook));

        this.GameId = reply.GameId;

        return this.GameId;
    }

    public async ValueTask EnterGame()
    {
        this.Logging($"GetCardPool: {this.playerName}: {this.GameId}: {this.PlayerId}");

        var cardPoolReply = await this.magiconionClient.GetCardPool(new GetCardPoolRequest(this.GameId));

        this.Logging($"reponse GetCardPool: {this.playerName}: {this.GameId}: {this.PlayerId}");

        var cardPool = cardPoolReply.Cards
            .Where(c => !c.IsToken)
            .ToArray();

        var deckCardIds = Enumerable.Range(0, 40)
            .Select(_ => Utility.RandomPick(cardPool).Id)
            .ToArray();

        this.Logging($"EnterGame: {this.playerName}: {this.GameId}: {this.PlayerId}");

        var reply = await this.magiconionClient.EnterGame(new EnterGameRequest(this.GameId, this.playerName, deckCardIds));

        this.Logging($"response EnterGame: {this.playerName}: {this.GameId}: {this.PlayerId}");

        this.PlayerId = reply.PlayerId;
    }

    public async ValueTask ReadyGame()
    {
        this.Logging($"ReadyGame: {this.playerName}: {this.GameId}: {this.PlayerId}");
        await this.magiconionClient.ReadyGame(new ReadyGameRequest(this.GameId, this.PlayerId));
    }

    public async ValueTask StartTurn()
    {
        await this.PlayAction(async () =>
        {
            var reply = await this.magiconionClient.StartTurn(new StartTurnRequest(this.GameId, this.PlayerId));

            this.currentContext = reply.GameContext;
        });
    }

    public async ValueTask PlayFromHand(CardId cardId)
    {
        await this.PlayAction(async () =>
        {
            var reply = await this.magiconionClient.PlayFromHand(new PlayFromHandRequest(this.GameId, this.PlayerId, cardId));

            if (!reply.Result)
            {
                this.LoggingError(reply.ErrorMessage);
                return;
            }

            this.currentContext = reply.GameContext;
        });
    }

    public async ValueTask PlayFromHand()
    {
        while (true)
        {
            var useableMp = this.currentContext.You.PublicPlayerInfo.CurrentMp;
            var (status, candidateCardIdList) = await this.magiconionClient.ListPlayableCardId(this.GameId);
            if (status != GameMasterStatusCode.OK)
            {
                this.LoggingError($"PlayFromHand に失敗！！ status={status}");
                return;
            }

            var cardId = Utility.RandomPick(candidateCardIdList);

            if (cardId == default)
            {
                return;
            }

            await this.PlayFromHand(cardId);
        }
    }

    public async ValueTask Attack(CardId attackCardId, CardId guardCardId)
    {
        await this.magiconionClient.AttackToCreature(new AttackToCreatureRequest(this.GameId, this.PlayerId, attackCardId, guardCardId));
    }

    public async ValueTask<(PlayerId[], CardId[])> ListAttackTargets(CardId cardId)
    {
        var apiResult = await this.magiconionClient.ListAttackTargets(this.GameId, cardId);

        return apiResult.Item2;
    }

    public async ValueTask AttackToOpponentPlayer(CardId attackCardId)
    {
        await this.magiconionClient.AttackToPlayer(new AttackToPlayerRequest(this.GameId, this.PlayerId, attackCardId, this.currentContext.Opponent.Id));
    }

    public async ValueTask Attack()
    {
        // フィールドのすべてのカードで敵に攻撃
        var allCreatures = this.currentContext
            .You.PublicPlayerInfo.Field
            .Where(card => card.Type == CardType.Creature);

        foreach (var attackCard in allCreatures)
        {
            var (status, (canAttackPlayerIdList, canAttackCardIdList)) = await this.magiconionClient.ListAttackTargets(this.GameId, attackCard.Id);
            if (status != GameMasterStatusCode.OK)
            {
                this.LoggingError($"攻撃に失敗！！ status={status}");
                return;
            }

            // 敵のモンスターがいる
            if (canAttackCardIdList.Any() && UnityEngine.Random.Range(0, 100) > 50)
            {
                var opponentCardId = canAttackCardIdList[0];

                var reply = await this.magiconionClient.AttackToCreature(new AttackToCreatureRequest(
                    this.GameId, this.PlayerId, attackCard.Id, opponentCardId
                    ));
                this.currentContext = reply.GameContext;
            }
            else if (canAttackPlayerIdList.Any())
            {
                var guardPlayerId = canAttackPlayerIdList[0];

                var reply = await this.magiconionClient.AttackToPlayer(new AttackToPlayerRequest(
                    this.GameId, this.PlayerId, attackCard.Id, guardPlayerId
                    ));
                this.currentContext = reply.GameContext;
            }
        }
    }

    public async ValueTask EndTurn()
    {
        await this.PlayAction(async () =>
        {
            var reply = await this.magiconionClient.EndTurn(new EndTurnRequest(this.GameId, this.PlayerId));
            this.currentContext = reply.GameContext;
        });
    }

    public async ValueTask<GameMasterStatusCode> AnswerChoice(Guid questionId, ChoiceCandidates choiceCandidates, int numPicks)
    {
        this.Logging($"answer: questionId={questionId}");

        var pickedPlayers = choiceCandidates.PlayerIdList.Take(numPicks).ToArray();
        numPicks -= pickedPlayers.Length;

        var pickedCards = choiceCandidates.CardList.Take(numPicks).Select(c => c.Id).ToArray();
        numPicks -= pickedCards.Length;

        var pickedCarddefs = choiceCandidates.CardDefList.Take(numPicks).Select(c => c.Id).ToArray();

        var answer = new ChoiceResult(
            pickedPlayers,
            pickedCards,
            pickedCarddefs
        );

        var result = await this.magiconionServiceClient.AnswerChoice(questionId, answer);

        return result;
    }

    public async ValueTask<GameMasterStatusCode> AnswerChoice(Guid questionId, ChoiceResult choiceResult)
    {
        this.Logging($"answer: questionId={questionId}");

        var result = await this.magiconionServiceClient.AnswerChoice(questionId, choiceResult);

        return result;
    }
}
