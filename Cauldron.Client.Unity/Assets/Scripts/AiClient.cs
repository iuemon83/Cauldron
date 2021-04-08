using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

public class AiClient
{
    private readonly Action<string> Logging;
    private readonly Action<string> LoggingError;

    private readonly Client client;

    public AiClient(string playerName, GameId gameId, ICauldronHubReceiver cauldronHubReceiver, Action<string> logInfo, Action<string> logError)
    {
        this.client = new Client(playerName, gameId, cauldronHubReceiver, logInfo, logError);
        this.Logging = logInfo;
        this.LoggingError = logError;
    }

    public async Task Destroy()
    {
        await this.client?.Destroy();
    }

    public async ValueTask Ready()
    {
        await this.client.EnterGame();
        await this.client.ReadyGame();
    }

    public async ValueTask PlayTurn()
    {
        await this.client.StartTurn();
        await this.PlayFromHand();
        await this.Attack();
        await this.client.EndTurn();
    }

    public async ValueTask PlayFromHand()
    {
        while (true)
        {
            var (status, candidateCardIdList) = await this.client.ListPlayableCardId();
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

            await this.client.PlayFromHand(cardId);
        }
    }

    public async ValueTask Attack()
    {
        // フィールドのすべてのカードで敵に攻撃
        var allCreatures = this.client.MyCreatureCards;

        foreach (var attackCardId in allCreatures)
        {
            var (attackTargetPlayerIdList, attackTargetCardIdList) = await this.client.ListAttackTargets(attackCardId);

            if (!attackTargetPlayerIdList.Any() && !attackTargetCardIdList.Any())
            {
                this.LoggingError($"攻撃対象なし！！");
                return;
            }

            // 敵のモンスターがいる
            if (attackTargetCardIdList.Any() && UnityEngine.Random.Range(0, 100) > 50)
            {
                var opponentCardId = attackTargetCardIdList[0];

                await this.client.Attack(attackCardId, opponentCardId);
            }
            else if (attackTargetPlayerIdList.Any())
            {
                await this.client.AttackToOpponentPlayer(attackCardId);
            }
        }
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

        var result = await this.client.AnswerChoice(questionId, answer);

        return result;
    }
}
