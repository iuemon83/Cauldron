using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using Grpc.Core;
using UnityEngine;

public class ListGameSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject GameList;
    [SerializeField]
    private GameObject GameListNodePrefab;
    [SerializeField]
    private ListDeckDialogController SelectDeckDialog;

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
            this.SelectDeckDialog.ShowDialog("Select you deck",
                async deck =>
                {
                    var holder = ConnectionHolder.Find();
                    await holder.Client.EnterGame(gameOutline.GameId, deck);

                    Utility.LoadAsyncScene(this, SceneNames.BattleScene);
                });
        });
    }

    public void OnOpenNewGameButtonClick()
    {
        Debug.Log("click open new game Button!");

        this.SelectDeckDialog.ShowDialog("Select you deck",
            async deck =>
            {
                var holder = ConnectionHolder.Find();
                var gameId = await holder.Client.OpenNewGame();

                try
                {
                    await holder.Client.EnterGame(gameId, deck);
                }
                catch (RpcException e)
                {
                    Debug.LogWarning(e);
                    return;
                }

                Utility.LoadAsyncScene(this, SceneNames.WatingRoomScene);
            });
    }

    public void OnReloadButtonClick()
    {
        Debug.Log("click reload Button!");

        this.RefreshGameList();
    }

    public void OnDeckButtonClick()
    {
        Utility.LoadAsyncScene(this, SceneNames.ListDeckScene);
    }

    public void OnVsAiButtonClick()
    {
        this.SelectDeckDialog.ShowDialog("Select you deck",
            myDeck =>
            {
                this.SelectDeckDialog.ShowDialog("Select AI deck",
                    aiDeck =>
                    {
                        this.battleAi(myDeck, aiDeck);
                    });
            });
    }

    private async void battleAi(IDeck myDeck, IDeck aiDeck)
    {
        var holder = ConnectionHolder.Find();
        var gameId = await holder.Client.OpenNewGame();

        try
        {
            await holder.Client.EnterGame(gameId, myDeck);
        }
        catch (RpcException e)
        {
            Debug.LogWarning(e);
            return;
        }

        Utility.LoadAsyncScene(this, SceneNames.BattleScene, () =>
        {
            var aiClientController = FindObjectOfType<AiClientController>();
            aiClientController.StartClient(gameId, aiDeck);
        });
    }
}
