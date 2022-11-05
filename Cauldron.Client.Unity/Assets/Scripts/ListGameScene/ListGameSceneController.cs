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
                        // �Q�[���J�n�O�ɃG���[
                        await holder.Client.LeaveRoom();

                        this.DisplayEnterGameErrorDialog(reply.StatusCode);
                        return;
                    }

                    await Utility.LoadAsyncScene(SceneNames.BattleScene, async () =>
                    {
                        // �ΐ��ʂ�������
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
                        // �Q�[���J�n�O�ɃG���[
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
                // �ΐ��ʂ�������
                var battleSceneController = FindObjectOfType<BattleSceneController>();
                await battleSceneController.Init();
            });
        });

        var title = "�ΐ푊���҂��Ă��܂�...";
        var message = "�ΐ푊���҂��Ă��܂�...";
        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.OnlyCancel,
            onCancelAction: async () =>
            {
                // �L�����Z��
                disposable?.Dispose();
                await holder.Client.LeaveRoom();
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    private void DisplayOpenNewGameErrorDialog(OpenNewRoomReply.StatusCodeValue statusCode)
    {
        var title = "�G���[���������܂����B";
        var message = statusCode switch
        {
            OpenNewRoomReply.StatusCodeValue.InvalidDeck => @"�I�������f�b�L�̓��e������������܂���B�f�b�L�ҏW��ʂ��m�F���Ă��������B
�E�f�b�L����������Ȃ� or �����Ă���B
�E1��ނ̃J�[�h���f�b�L�ɓ�����閇���𒴉߂��Ă���B",
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
        var title = "�G���[���������܂����B";
        var message = statusCode switch
        {
            JoinRoomReply.StatusCodeValue.RoomIsFull => "�����ɓ���܂���ł����B",
            JoinRoomReply.StatusCodeValue.InvalidDeck => @"�I�������f�b�L�̓��e������������܂���B�f�b�L�ҏW��ʂ��m�F���Ă��������B
�E�f�b�L����������Ȃ� or �����Ă���B
�E1��ނ̃J�[�h���f�b�L�ɓ�����閇���𒴉߂��Ă���B",
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
        var title = "�G���[���������܂����B";
        var message = "�G���[���������܂����B";

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
        // �������̐ڑ�������������----

        var holder = ConnectionHolder.Find();
        var reply = await holder.Client.OpenNewRoom("", myDeck);

        //var reply = await holder.Client.EnterGame(myDeck);
        if (reply.StatusCode != OpenNewRoomReply.StatusCodeValue.Ok)
        {
            // �Q�[���J�n�O�ɃG���[
            await holder.Client.LeaveRoom();

            this.DisplayOpenNewGameErrorDialog(reply.StatusCode);
            return;
        }

        // �������̐ڑ����������܂�----

        await Utility.LoadAsyncScene(SceneNames.BattleScene, async () =>
        {
            // �ΐ��ʂ�������
            var battleSceneController = FindObjectOfType<BattleSceneController>();
            await battleSceneController.Init();

            // AI����ڑ�����
            var aiClientController = FindObjectOfType<AiClientController>();
            await aiClientController.StartClient(reply.GameId, aiDeck);
        });
    }
}
