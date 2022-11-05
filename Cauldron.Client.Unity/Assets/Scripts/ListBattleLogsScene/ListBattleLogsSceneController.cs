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
    private static ListGameHistoriesRequest searchConditionCache;

    /// <summary>
    /// リプレイを再生してから戻ってきたときのためのもの
    /// それ以外では利用しない
    /// </summary>
    private static float scrollPositionCache = 1;

    [SerializeField]
    private GameObject canvas = default;
    [SerializeField]
    private ScrollRect battleLogList = default;
    [SerializeField]
    private ListBattleLogNodeController listNodePrefab = default;
    [SerializeField]
    private StartBattleLogDialogController startBattleLogDialogController = default;
    [SerializeField]
    private Toggle onlyMyReplaysToggle = default;
    [SerializeField]
    private InputField searchByGameIdInputField = default;
    [SerializeField]
    private ToastController toastPrefab = default;
    [SerializeField]
    private LoadingViewController loadingViewPrefab = default;

    // Start is called before the first frame update
    async void Start()
    {
        this.DisplaySerchConditionCache();

        await this.RefreshReplays();

        this.battleLogList.verticalNormalizedPosition = scrollPositionCache;
        scrollPositionCache = 1;
    }

    private void DisplaySerchConditionCache()
    {
        if (searchConditionCache == null)
        {
            return;
        }

        this.searchByGameIdInputField.text = searchConditionCache.GameIdList.Any()
            ? searchConditionCache.GameIdList[0].ToString()
            : "";

        this.onlyMyReplaysToggle.isOn = searchConditionCache.OnlyMyLogs;
    }

    private async UniTask RefreshReplays()
    {
        var loadingView = Instantiate(this.loadingViewPrefab);
        loadingView.Show(this.canvas);

        try
        {
            var gameIdList = new GameId[0];
            var searchKeyword = this.searchByGameIdInputField.text.Trim();
            if (searchKeyword != "")
            {
                if (Guid.TryParse(this.searchByGameIdInputField.text, out var gameGuid))
                {
                    var searchGameId = new GameId(gameGuid);
                    gameIdList = new[] { searchGameId };
                }
            }

            searchConditionCache = new ListGameHistoriesRequest(LocalData.ClientId, this.onlyMyReplaysToggle.isOn, gameIdList);
            var replays = await ConnectionHolder.Find().Client.ListGameHistories(searchConditionCache);
            if ((replays?.Length ?? 0) == 0)
            {
                Debug.LogError("リプレイが取得できない");
            }

            Debug.Log("リプレイ取得完了");

            this.DisplayReplays(replays);
        }
        finally
        {
            loadingView?.Hide();
        }
    }

    private void ClearReplaysList()
    {
        foreach (Transform child in this.battleLogList.content.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void DisplayReplays(IReadOnlyList<GameReplay> gameReplays)
    {
        this.ClearReplaysList();

        var i = 0;
        foreach (var replay in gameReplays.OrderByDescending(l => l.DateTime))
        {
            this.AddListNode(i, replay);
            i++;
        }

        this.battleLogList.verticalNormalizedPosition = 1;
    }

    private void AddListNode(int index, GameReplay gameReplay)
    {
        var playerList = gameReplay.Players
            .Select(p => (p.Id, p.Name))
            .ToArray();

        var controller = Instantiate(this.listNodePrefab, this.battleLogList.content.transform);
        controller.Set(index, gameReplay,
            () =>
            {
                var dialog = Instantiate(this.startBattleLogDialogController);
                dialog.Init(
                    playerList,
                    selectedPlayerId => this.ShowBattleLog(gameReplay, selectedPlayerId));
                dialog.transform.SetParent(this.canvas.transform, false);
            },
            () =>
            {
                var toast = Instantiate(this.toastPrefab);
                toast.Show(this.canvas, "");
            });
    }

    private async void ShowBattleLog(GameReplay gameReplay, PlayerId playerId)
    {
        scrollPositionCache = this.battleLogList.verticalNormalizedPosition;

        await Utility.LoadAsyncScene(SceneNames.BattleScene, async () =>
        {
            var cardpool = await ConnectionHolder.Find().Client.GetCardPool(gameReplay.GameId);

            // リプレイ画面を初期化
            var replaySceneController = FindObjectOfType<ReplaySceneController>();
            await replaySceneController.Init(gameReplay, playerId, cardpool);
        });
    }

    public async void OnClearButtonClick()
    {
        if (this.searchByGameIdInputField.text == "")
        {
            return;
        }

        this.searchByGameIdInputField.text = "";
        await this.RefreshReplays();
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
