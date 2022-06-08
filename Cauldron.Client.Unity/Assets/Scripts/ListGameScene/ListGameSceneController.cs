using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects;
using Grpc.Core;
using System;
using UniRx;
using UnityEngine;

public class ListGameSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject GameList = default;
    [SerializeField]
    private GameObject GameListNodePrefab = default;
    [SerializeField]
    private ListDeckDialogController SelectDeckDialog = default;
    [SerializeField]
    private Canvas canvas = default;
    [SerializeField]
    private ConfirmDialogController confirmDialogController = default;
    [SerializeField]
    private AudioSource audioSource = default;

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
            this.SelectDeckDialog.ShowDialog(true,
                async deck =>
                {
                    var holder = ConnectionHolder.Find();
                    var reply = await holder.Client.EnterGame(gameOutline.GameId, deck);
                    if (reply.StatusCode != EnterGameReply.StatusCodeValue.Ok)
                    {
                        // ゲーム開始前にエラー
                        await holder.Client.LeaveGame();

                        this.DisplayEnterGameErrorDialog(reply.StatusCode);
                        return;
                    }

                    await Utility.LoadAsyncScene(SceneNames.BattleScene);
                });
        });
    }

    public async void OnBattleLogButtonClick()
    {
        await Utility.LoadAsyncScene(SceneNames.ListBattleLogsScene);
    }

    public void OnOpenNewGameButtonClick()
    {
        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

        this.SelectDeckDialog.ShowDialog(true,
            async deck =>
            {
                var holder = ConnectionHolder.Find();
                await holder.Client.OpenNewGame();

                var reply = await holder.Client.EnterGame(deck);
                if (reply.StatusCode != EnterGameReply.StatusCodeValue.Ok)
                {
                    // ゲーム開始前にエラー
                    await holder.Client.LeaveGame();

                    this.DisplayEnterGameErrorDialog(reply.StatusCode);
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

    private void DisplayEnterGameErrorDialog(EnterGameReply.StatusCodeValue statusCode)
    {
        var title = "エラーが発生しました。";
        var message = statusCode switch
        {
            EnterGameReply.StatusCodeValue.RoomIsFull => "部屋に入れませんでした。",
            EnterGameReply.StatusCodeValue.InvalidDeck => @"選択したデッキの内容が正しくありません。デッキ編集画面より確認してください。
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

    public void OnReloadButtonClick()
    {
        Debug.Log("click reload Button!");

        this.RefreshGameList();
    }

    public async void OnDeckButtonClick()
    {
        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

        await Utility.LoadAsyncScene(SceneNames.ListDeckScene);
    }

    public void OnVsAiButtonClick()
    {
        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

        this.SelectDeckDialog.ShowDialog(true,
            myDeck =>
            {
                this.PlayAudio(SeAudioCache.SeAudioType.Ok);

                this.SelectDeckDialog.ShowDialog(false,
                    aiDeck =>
                    {
                        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

                        this.BattleAi(myDeck, aiDeck);
                    },
                    () =>
                    {
                        this.PlayAudio(SeAudioCache.SeAudioType.Cancel);
                    });
            },
            () =>
            {
                this.PlayAudio(SeAudioCache.SeAudioType.Cancel);
            });
    }

    private async void BattleAi(IDeck myDeck, IDeck aiDeck)
    {
        // 自分側の接続処理ここから----

        var holder = ConnectionHolder.Find();
        var gameId = await holder.Client.OpenNewGame();

        var reply = await holder.Client.EnterGame(myDeck);
        if (reply.StatusCode != EnterGameReply.StatusCodeValue.Ok)
        {
            // ゲーム開始前にエラー
            await holder.Client.LeaveGame();

            this.DisplayEnterGameErrorDialog(reply.StatusCode);
            return;
        }

        // 自分側の接続処理ここまで----

        await Utility.LoadAsyncScene(SceneNames.BattleScene, async () =>
        {
            // AI側を接続する
            var aiClientController = FindObjectOfType<AiClientController>();
            await aiClientController.StartClient(gameId, aiDeck);
        });
    }

    private void PlayAudio(SeAudioCache.SeAudioType audioType)
    {
        var (b, a) = SeAudioCache.GetOrInit(audioType);
        if (b)
        {
            this.audioSource.PlayOneShot(a);
        }
    }
}
