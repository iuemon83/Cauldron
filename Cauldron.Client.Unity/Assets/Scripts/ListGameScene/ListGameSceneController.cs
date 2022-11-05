using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects;
using System;
using UniRx;
using UnityEngine;

public class ListGameSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject GameList = default;
    [SerializeField]
    private GameListNodeController GameListNodePrefab = default;
    [SerializeField]
    private CreateRoomDialogController SelectDeckDialog = default;
    [SerializeField]
    private Canvas canvas = default;
    [SerializeField]
    private ConfirmDialogController confirmDialogController = default;

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

    private void AddListNode(RoomOutline gameOutline)
    {
        var controller = Instantiate(this.GameListNodePrefab, this.listContent.transform);
        controller.Set(gameOutline, () =>
        {
            this.SelectDeckDialog.ShowYouJoinRoomDialog(
                async (deck, message) =>
                {
                    var holder = ConnectionHolder.Find();
                    var reply = await holder.Client.JoinRoom(gameOutline.GameId, deck);
                    if (reply.StatusCode != JoinRoomReply.StatusCodeValue.Ok)
                    {
                        // ゲーム開始前にエラー
                        await holder.Client.LeaveRoom();

                        this.DisplayEnterGameErrorDialog(reply.StatusCode);
                        return;
                    }

                    await Utility.LoadAsyncScene(SceneNames.BattleScene, async () =>
                    {
                        // 対戦画面を初期化
                        var battleSceneController = FindObjectOfType<BattleSceneController>();
                        await battleSceneController.Init();
                    });
                });
        });
    }

    public async void OnBattleLogButtonClick()
    {
        await Utility.LoadAsyncScene(SceneNames.ListBattleLogsScene);
    }

    public void OnOpenNewGameButtonClick()
    {
        AudioController.CreateOrFind().PlaySe(SeAudioCache.SeAudioType.Ok);

        this.SelectDeckDialog.ShowNewRoomDialog(
            async (deck, message) =>
            {
                var holder = ConnectionHolder.Find();

                try
                {
                    var reply = await holder.Client.OpenNewRoom(message, deck);

                    //var reply = await holder.Client.EnterGame(deck);
                    if (reply.StatusCode != OpenNewRoomReply.StatusCodeValue.Ok)
                    {
                        // ゲーム開始前にエラー
                        await holder.Client.LeaveRoom();

                        this.DisplayOpenNewGameErrorDialog(reply.StatusCode);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);

                    this.DisplayUnknownErrorDialog();
                    return;
                }

                this.ShowWaitDialog(holder);
            });
    }

    private void ShowWaitDialog(ConnectionHolder holder)
    {
        IDisposable disposable = default;
        disposable = holder.Receiver.OnJoinGame.Subscribe(async _ =>
        {
            disposable?.Dispose();
            await Utility.LoadAsyncScene(SceneNames.BattleScene, async () =>
            {
                // 対戦画面を初期化
                var battleSceneController = FindObjectOfType<BattleSceneController>();
                await battleSceneController.Init();
            });
        });

        var title = "対戦相手を待っています...";
        var message = "対戦相手を待っています...";
        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.OnlyCancel,
            onCancelAction: async () =>
            {
                // キャンセル
                disposable?.Dispose();
                await holder.Client.LeaveRoom();
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    private void DisplayOpenNewGameErrorDialog(OpenNewRoomReply.StatusCodeValue statusCode)
    {
        var title = "エラーが発生しました。";
        var message = statusCode switch
        {
            OpenNewRoomReply.StatusCodeValue.InvalidDeck => @"選択したデッキの内容が正しくありません。デッキ編集画面より確認してください。
・デッキ枚数が足りない or 超えている。
・1種類のカードをデッキに入れられる枚数を超過している。",
            _ => throw new NotImplementedException()
        };

        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.OnlyCancel,
            onCancelAction: () =>
            {
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    private void DisplayEnterGameErrorDialog(JoinRoomReply.StatusCodeValue statusCode)
    {
        var title = "エラーが発生しました。";
        var message = statusCode switch
        {
            JoinRoomReply.StatusCodeValue.RoomIsFull => "部屋に入れませんでした。",
            JoinRoomReply.StatusCodeValue.InvalidDeck => @"選択したデッキの内容が正しくありません。デッキ編集画面より確認してください。
・デッキ枚数が足りない or 超えている。
・1種類のカードをデッキに入れられる枚数を超過している。",
            _ => throw new NotImplementedException()
        };

        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.OnlyCancel,
            onCancelAction: () =>
            {
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    private void DisplayUnknownErrorDialog()
    {
        var title = "エラーが発生しました。";
        var message = "エラーが発生しました。";

        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.OnlyCancel,
            onCancelAction: () =>
            {
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    public void OnReloadButtonClick()
    {
        Debug.Log("click reload Button!");

        this.RefreshGameList();
    }

    public async void OnDeckButtonClick()
    {
        AudioController.CreateOrFind().PlaySe(SeAudioCache.SeAudioType.Ok);

        await Utility.LoadAsyncScene(SceneNames.ListDeckScene);
    }

    public void OnSoloButtonClick()
    {
        AudioController.CreateOrFind().PlaySe(SeAudioCache.SeAudioType.Ok);

        this.SelectDeckDialog.ShowYouJoinRoomDialog(
            (myDeck, message) =>
            {
                AudioController.CreateOrFind().PlaySe(SeAudioCache.SeAudioType.Ok);

                this.SelectDeckDialog.ShowAiJoinRoomDialog(
                    (aiDeck, message) =>
                    {
                        AudioController.CreateOrFind().PlaySe(SeAudioCache.SeAudioType.Ok);

                        this.SoloBattle(myDeck, aiDeck);
                    },
                    () =>
                    {
                        AudioController.CreateOrFind().PlaySe(SeAudioCache.SeAudioType.Cancel);
                    });
            },
            () =>
            {
                AudioController.CreateOrFind().PlaySe(SeAudioCache.SeAudioType.Cancel);
            });
    }

    private async void SoloBattle(IDeck myDeck, IDeck aiDeck)
    {
        // 自分側の接続処理ここから----

        var holder = ConnectionHolder.Find();
        var reply = await holder.Client.OpenNewRoom("", myDeck);

        //var reply = await holder.Client.EnterGame(myDeck);
        if (reply.StatusCode != OpenNewRoomReply.StatusCodeValue.Ok)
        {
            // ゲーム開始前にエラー
            await holder.Client.LeaveRoom();

            this.DisplayOpenNewGameErrorDialog(reply.StatusCode);
            return;
        }

        // 自分側の接続処理ここまで----

        await Utility.LoadAsyncScene(SceneNames.BattleScene, async () =>
        {
            // 対戦画面を初期化
            var battleSceneController = FindObjectOfType<BattleSceneController>();
            await battleSceneController.Init();

            // AI側を接続する
            var aiClientController = FindObjectOfType<AiClientController>();
            await aiClientController.StartClient(reply.GameId, aiDeck);
        });
    }
}
