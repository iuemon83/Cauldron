using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using Grpc.Core;
using System;
using UniRx;
using UnityEngine;

public class ListGameSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject GameList;
    [SerializeField]
    private GameObject GameListNodePrefab;
    [SerializeField]
    private ListDeckDialogController SelectDeckDialog;
    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private ConfirmDialogController confirmDialogController;

    private Transform listContent;

    // Start is called before the first frame update
    void Start()
    {
        this.listContent = this.GameList.transform.Find("Viewport").transform.Find("Content");

        this.RefreshGameList();
    }

    private async void RefreshGameList()
    {
        // clear list
        foreach (Transform child in this.listContent.transform)
        {
            Destroy(child.gameObject);
        }

        var connectionHolder = ConnectionHolder.Find();
        var openGames = await connectionHolder.Client.ListOpenGames();

        foreach (var n in openGames)
        {
            this.AddListNode(n);
        }
    }

    private void AddListNode(GameOutline gameOutline)
    {
        var node = Instantiate(this.GameListNodePrefab, this.listContent.transform);
        var controller = node.GetComponent<GameListNodeController>();
        controller.Set(gameOutline, () =>
        {
            this.SelectDeckDialog.ShowDialog("Select your deck",
                async deck =>
                {
                    var holder = ConnectionHolder.Find();
                    await holder.Client.EnterGame(gameOutline.GameId, deck);

                    await Utility.LoadAsyncScene(SceneNames.BattleScene);
                });
        });
    }

    public void OnOpenNewGameButtonClick()
    {
        this.SelectDeckDialog.ShowDialog("Select your deck",
            async deck =>
            {
                var holder = ConnectionHolder.Find();
                await holder.Client.OpenNewGame();

                try
                {
                    await holder.Client.EnterGame(deck);
                }
                catch (RpcException e)
                {
                    Debug.LogWarning(e);
                    return;
                }

                IDisposable disposable = default;
                disposable = holder.Receiver.OnJoinGame.Subscribe(async _ =>
                {
                    disposable?.Dispose();
                    await Utility.LoadAsyncScene(SceneNames.BattleScene);
                });

                var title = "対戦相手を待っています...";
                var message = "対戦相手を待っています...";
                var dialog = Instantiate(this.confirmDialogController);
                dialog.Init(title, message, ConfirmDialogController.DialogType.OnlyCancel,
                    onCancelAction: async () =>
                    {
                        // キャンセル
                        disposable?.Dispose();
                        await holder.Client.LeaveGame();
                    });
                dialog.transform.SetParent(this.canvas.transform, false);
            });
    }

    public void OnReloadButtonClick()
    {
        Debug.Log("click reload Button!");

        this.RefreshGameList();
    }

    public async UniTaskVoid OnDeckButtonClick()
    {
        await Utility.LoadAsyncScene(SceneNames.ListDeckScene);
    }

    public void OnVsAiButtonClick()
    {
        this.SelectDeckDialog.ShowDialog("Select your deck",
            myDeck =>
            {
                this.SelectDeckDialog.ShowDialog("Select AI deck",
                    aiDeck =>
                    {
                        this.BattleAi(myDeck, aiDeck);
                    });
            });
    }

    private async void BattleAi(IDeck myDeck, IDeck aiDeck)
    {
        // 自分側の接続処理ここから----

        var holder = ConnectionHolder.Find();
        var gameId = await holder.Client.OpenNewGame();

        try
        {
            await holder.Client.EnterGame(myDeck);
        }
        catch (RpcException e)
        {
            Debug.LogWarning(e);
            return;
        }

        // 自分側の接続処理ここまで----

        await Utility.LoadAsyncScene(SceneNames.BattleScene, () =>
        {
            // AI側を接続する
            var aiClientController = FindObjectOfType<AiClientController>();
            aiClientController.StartClient(gameId, aiDeck);
        });
    }
}
