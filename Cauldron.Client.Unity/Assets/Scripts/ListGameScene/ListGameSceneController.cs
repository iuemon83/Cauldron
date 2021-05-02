using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ListGameSceneController : MonoBehaviour
{
    public GameObject GameList;
    public GameObject GameListNodePrefab;

    private Transform listContent;

    // Start is called before the first frame update
    void Start()
    {
        this.listContent = this.GameList.transform.Find("Viewport").transform.Find("Content");

        this.RefreshGameList();
    }

    private IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        var asyncLoad = SceneManager.LoadSceneAsync(SceneNames.WatingRoomScene.ToString());

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private async void RefreshGameList()
    {
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
    }

    public async void OnOpenNewGameButtonClick()
    {
        Debug.Log("click open new game Button!");

        var holder = ConnectionHolder.Find();
        var gameId = await holder.Client.OpenNewGame();
        await holder.Client.EnterGame(gameId);

        StartCoroutine(LoadYourAsyncScene());
    }

    public void OnReloadButtonClick()
    {
        Debug.Log("click reload Button!");

        this.RefreshGameList();
    }
}
