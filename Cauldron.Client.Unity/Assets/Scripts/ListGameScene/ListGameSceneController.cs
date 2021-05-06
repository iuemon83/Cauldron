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
        controller.Set(gameOutline);
        controller.OnJoinButtonClickAction = () =>
        {
            this.SelectDeckDialog.OnOkButtonClickAction = async deck =>
            {
                var holder = ConnectionHolder.Find();
                await holder.Client.EnterGame(gameOutline.GameId, deck);

                Utility.LoadAsyncScene(this, SceneNames.BattleScene);
            };
            this.SelectDeckDialog.ShowDialog();
        };
    }

    public void OnOpenNewGameButtonClick()
    {
        Debug.Log("click open new game Button!");

        this.SelectDeckDialog.OnOkButtonClickAction = async deck =>
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
        };
        this.SelectDeckDialog.ShowDialog();
    }

    public void OnReloadButtonClick()
    {
        Debug.Log("click reload Button!");

        this.RefreshGameList();
    }
}
