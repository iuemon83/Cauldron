using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using UnityEngine;

public class AiClient
{
    public static async UniTask<AiClient> Factory(string serverAddress, string playerName, GameId gameId, ICauldronHubReceiver cauldronHubReceiver, Action<string> logInfo, Action<string> logError)
    {
        var c = new AiClient(gameId, logInfo, logError);
        await c.Connect(serverAddress, playerName, cauldronHubReceiver, logInfo, logError);

        return c;
    }

    public PlayerId PlayerId { get; private set; }

    private readonly Action<string> Logging;
    private readonly Action<string> LoggingError;

    private readonly GameId gameId;

    private Client client;

    private AiClient(GameId gameId, Action<string> logInfo, Action<string> logError)
    {
        this.gameId = gameId;

        this.Logging = logInfo;
        this.LoggingError = logError;
    }

    private async UniTask Connect(string serverAddress, string playerName,
        ICauldronHubReceiver cauldronHubReceiver, Action<string> logInfo, Action<string> logError)
    {
        this.client = await Client.Factory(serverAddress, playerName, cauldronHubReceiver, logInfo, logError);
    }

    public async UniTask Destroy()
    {
        if (this.client == null)
        {
            return;
        }

        await this.client.Destroy();
    }

    public async UniTask Ready(IDeck deck)
    {
        this.PlayerId = await this.client.EnterGame(this.gameId, deck);
        await this.client.ReadyGame();
    }

    public async UniTask PlayTurn()
    {
        await this.client.StartTurn();
        await this.PlayFromHand();
        await this.Attack();
        await this.client.EndTurn();
    }

    public async UniTask PlayFromHand()
    {
        while (true)
        {
            var cardId = Utility.RandomPick(this.client.PlayableCardIdList);

            if (cardId == default)
            {
                return;
            }

            var playResult = await this.client.PlayFromHand(cardId);
            if (!playResult)
            {
                // 勝負がついている
                return;
            }
        }
    }

    public async UniTask Attack()
    {
        // フィールドのすべてのカードで敵に攻撃
        var allCreatures = this.client.MyCreatureCards;

        foreach (var attackCardId in allCreatures)
        {
            var (attackTargetPlayerIdList, attackTargetCardIdList) = await this.client.ListAttackTargets(attackCardId);

            if (!attackTargetPlayerIdList.Any() && !attackTargetCardIdList.Any())
            {
                this.Logging($"攻撃できないか攻撃対象なし！！");
                continue;
            }

            // 敵のモンスターがいる
            if (attackTargetCardIdList.Any() && UnityEngine.Random.Range(0, 100) > 40)
            {
                var index = UnityEngine.Random.Range(0, attackTargetCardIdList.Length);

                var opponentCardId = attackTargetCardIdList[index];

                await this.client.Attack(attackCardId, opponentCardId);
            }
            else if (attackTargetPlayerIdList.Any())
            {
                await this.client.AttackToOpponentPlayer(attackCardId);
            }
        }
    }

    public async UniTask<GameMasterStatusCode> AnswerChoice(Guid questionId, ChoiceCandidates choiceCandidates, int numPicks)
    {
        this.Logging($"answer: questionId={questionId}");

        var pickedPlayers = choiceCandidates.PlayerIdList.Take(numPicks).ToArray();
        numPicks -= pickedPlayers.Length;

        var pickedCards = choiceCandidates.CardList.Take(numPicks).Select(c => c.Id).ToArray();
        numPicks -= pickedCards.Length;

        var pickedCarddefs = choiceCandidates.CardDefList.Take(numPicks).Select(c => c.Id).ToArray();

        var answer = new ChoiceAnswer(
            pickedPlayers,
            pickedCards,
            pickedCarddefs
        );

        var result = await this.client.AnswerChoice(questionId, answer);

        return result;
    }
}
