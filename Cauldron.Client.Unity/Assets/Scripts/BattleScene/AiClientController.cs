using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.Services;
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

    public async void StartClient(GameId gameId, IDeck deck)
    {
        this.client = new AiClient(LocalData.ServerAddress, this.playerName, gameId, this, Debug.Log, Debug.LogError);

        await this.client.Ready(deck);
    }

    async void ICauldronHubReceiver.OnStartTurn(GameContext gameContext, PlayerId playerId)
    {
        if (this.client.PlayerId == playerId)
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

    void ICauldronHubReceiver.OnAddCard(GameContext gameContext, AddCardNotifyMessage addCardNotifyMessage)
    {
    }

    void ICauldronHubReceiver.OnExcludeCard(GameContext gameContext, ExcludeCardNotifyMessage message)
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

    void ICauldronHubReceiver.OnJoinGame()
    {
    }

    void ICauldronHubReceiver.OnBattle(GameContext gameContext, BattleNotifyMessage message)
    {
    }
}