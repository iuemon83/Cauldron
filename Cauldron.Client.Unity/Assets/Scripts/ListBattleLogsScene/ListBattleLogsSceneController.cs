using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ListBattleLogsSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject canvas = default;
    [SerializeField]
    private GameObject battleLogList = default;
    [SerializeField]
    private ListBattleLogNodeController listNodePrefab = default;
    [SerializeField]
    private StartBattleLogDialogController startBattleLogDialogController = default;
    [SerializeField]
    private Toggle onlyMyReplaysToggle = default;
    [SerializeField]
    private InputField searchByGameIdInputField = default;

    private Transform listContent;

    private Dictionary<GameId, LocalBattleLog> localBattlelogsByGameId = new Dictionary<GameId, LocalBattleLog>();

    // Start is called before the first frame update
    async void Start()
    {
        this.listContent = this.battleLogList.transform.Find("Viewport").transform.Find("Content");

        this.localBattlelogsByGameId = LocalData.LoadLocalData().BattleLogs
            .ToDictionary(l => GameId.Parse(l.GameIdText));

        await this.RefreshReplays();
    }

    private async UniTask RefreshReplays()
    {
        var gameIdList = this.onlyMyReplaysToggle.isOn
            ? this.localBattlelogsByGameId.Keys.ToArray()
            : default;

        var searchKeyword = this.searchByGameIdInputField.text.Trim();
        if (searchKeyword != "")
        {
            if (Guid.TryParse(this.searchByGameIdInputField.text, out var gameGuid))
            {
                var searchGameId = new GameId(gameGuid);
                gameIdList = gameIdList == null
                    ? new[] { searchGameId }
                    : gameIdList.Where(gameId => gameId == searchGameId).ToArray();
            }
        }

        var request = new ListGameHistoriesRequest(gameIdList);
        var replays = await ConnectionHolder.Find().Client.ListGameHistories(request);
        if ((replays?.Length ?? 0) == 0)
        {
            Debug.LogError("リプレイが取得できない");
        }

        Debug.Log("リプレイ取得完了");

        this.DisplayReplays(replays);
    }

    private void ClearReplaysList()
    {
        foreach (Transform child in this.listContent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void DisplayReplays(IReadOnlyList<GameReplay> gameReplays)
    {
        this.ClearReplaysList();

        foreach (var replay in gameReplays.OrderByDescending(l => l.DateTime))
        {
            if (this.localBattlelogsByGameId.TryGetValue(replay.GameId, out var localLog))
            {
                this.AddListNode(replay, localLog);
            }
            else
            {
                this.AddListNode(replay);
            }
        }
    }

    private void AddListNode(GameReplay gameReplay, LocalBattleLog localLog)
    {
        var playerList = gameReplay.PlayerIdList.Select(pid =>
        {
            var name = pid.ToString() == localLog.YouIdText
                ? localLog.YouName
                : localLog.OpponentName;

            return (pid, name);
        })
            .ToArray();

        var controller = Instantiate(this.listNodePrefab, this.listContent.transform);
        controller.Set(gameReplay, () =>
        {
            var dialog = Instantiate(this.startBattleLogDialogController);
            dialog.Init(
                playerList,
                selectedPlayerId => this.ShowBattleLog(gameReplay, selectedPlayerId));
            dialog.transform.SetParent(this.canvas.transform, false);
        },
        localLog);
    }

    private void AddListNode(GameReplay gameReplay)
    {
        var playerList = gameReplay.PlayerIdList.Zip(new[] { "Player1", "Player2" },
            (a, b) => (a, b))
            .ToArray();


        var controller = Instantiate(this.listNodePrefab, this.listContent.transform);
        controller.Set(gameReplay, () =>
        {
            var dialog = Instantiate(this.startBattleLogDialogController);
            dialog.Init(
                playerList,
                selectedPlayerId => this.ShowBattleLog(gameReplay, selectedPlayerId));
            dialog.transform.SetParent(this.canvas.transform, false);
        });
    }

    private async void ShowBattleLog(GameReplay gameReplay, PlayerId playerId)
    {
        await Utility.LoadAsyncScene(SceneNames.BattleScene, async () =>
        {
            // リプレイ画面を初期化
            var replaySceneController = FindObjectOfType<ReplaySceneController>();
            await replaySceneController.Init(gameReplay, playerId);
        });
    }

    public async void OnCloseButtonClick()
    {
        await Utility.LoadAsyncScene(SceneNames.ListGameScene);
    }

    public async void OnSearchByGameIdButtonClick()
    {
        await this.RefreshReplays();
    }

    public async void OnOnlyMyReplaysToggleChanged()
    {
        await this.RefreshReplays();
    }
}
