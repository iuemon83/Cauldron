using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using System.Threading.Tasks;
using UnityEngine;

public class AiClientController : MonoBehaviour, ICauldronHubReceiver
{
    private AiClient client;
    private readonly string playerName = "AI";

    void Start()
    {
        this.StartClient();
    }

    async void OnDestroy()
    {
        await this.client?.Destroy();
    }

    async void StartClient()
    {
        var gameId = await this.GetGameId();

        this.client = new AiClient(Config.ServerAddress, this.playerName, gameId, this, Debug.Log, Debug.LogError);

        await this.client.Ready();
    }

    async ValueTask<GameId> GetGameId()
    {
        return await Task.Run(() =>
        {
            while (Utility.GameId == null) { }
            return Utility.GameId.Value;
        });
    }

    async void ICauldronHubReceiver.OnStartTurn(GameContext gameContext)
    {
        // 自分のターン
        Debug.Log("ターン開始: " + this.playerName);
        await this.client.PlayTurn();
    }

    async void ICauldronHubReceiver.OnChoiceCards(ChoiceCardsMessage choiceCardsMessage)
    {
        Debug.Log($"{nameof(ICauldronHubReceiver.OnChoiceCards)}");

        var result = await this.client.AnswerChoice(choiceCardsMessage.QuestionId, choiceCardsMessage.ChoiceCandidates, choiceCardsMessage.NumPicks);

        Debug.Log($"result: {result}");
    }

    void ICauldronHubReceiver.OnReady(GameContext gameContext)
    {
    }

    void ICauldronHubReceiver.OnStartGame(GameContext gameContext)
    {
    }

    void ICauldronHubReceiver.OnGameOver(GameContext gameContext)
    {
    }

    void ICauldronHubReceiver.OnAddCard(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage)
    {
    }

    void ICauldronHubReceiver.OnMoveCard(GameContext gameContext, MoveCardNotifyMessage moveCardNotifyMessage)
    {
    }

    void ICauldronHubReceiver.OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage modifyCardNotifyMessage)
    {
    }

    void ICauldronHubReceiver.OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage modifyPlayerNotifyMessage)
    {
    }

    void ICauldronHubReceiver.OnDamage(GameContext gameContext, DamageNotifyMessage damageNotifyMessage)
    {
    }
}