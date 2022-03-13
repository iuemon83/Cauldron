using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AiClientController : MonoBehaviour, ICauldronHubReceiver
{
    private AiClient client;
    private readonly string playerName = "AI";

    async void OnDestroy()
    {
        if (this.client != null)
        {
            await this.client.Destroy();
        }
    }

    public async UniTask StartClient(GameId gameId, IDeck deck)
    {
        this.client = await AiClient.Factory(LocalData.ServerAddress, this.playerName, gameId,
            this, Debug.Log, Debug.LogError);

        await this.client.Ready(deck);
    }

    async void ICauldronHubReceiver.OnStartTurn(GameContext gameContext, StartTurnNotifyMessage message)
    {
        if (this.client.PlayerId == message.PlayerId)
        {
            // 自分のターン
            Debug.Log("ターン開始: " + this.playerName);
            await this.client.PlayTurn();
        }
    }

    async void ICauldronHubReceiver.OnAsk(AskMessage choiceCardsMessage)
    {
        Debug.Log($"{nameof(ICauldronHubReceiver.OnAsk)}");

        var result = await this.client.AnswerChoice(choiceCardsMessage.QuestionId, choiceCardsMessage.ChoiceCandidates, choiceCardsMessage.NumPicks);

        Debug.Log($"result: {result}");
    }

    void ICauldronHubReceiver.OnReady()
    {
    }

    void ICauldronHubReceiver.OnStartGame()
    {
    }

    void ICauldronHubReceiver.OnPlayCard(GameContext gameContext, PlayCardNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnAddCard(GameContext gameContext, AddCardNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnExcludeCard(GameContext gameContext, ExcludeCardNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnMoveCard(GameContext gameContext, MoveCardNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnModifyCard(GameContext gameContext, ModifyCardNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnModifyPlayer(GameContext gameContext, ModifyPlayerNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnDamage(GameContext gameContext, DamageNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnJoinGame()
    {
    }

    void ICauldronHubReceiver.OnBattleStart(GameContext gameContext, BattleNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnBattleEnd(GameContext gameContext, BattleNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnEndGame(GameContext gameContext, EndGameNotifyMessage message)
    {
    }

    void ICauldronHubReceiver.OnModifyCounter(GameContext gameContext, ModifyCounterNotifyMessage message)
    {
    }
}
