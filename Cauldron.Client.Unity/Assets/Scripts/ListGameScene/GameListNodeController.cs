using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameListNodeController : MonoBehaviour
{
    public Text OwnerNameText;

    private GameOutline gameOutline;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Set(GameOutline gameOutline)
    {
        this.gameOutline = gameOutline;
        this.OwnerNameText.text = this.gameOutline.OwnerName;
    }

    /// <summary>
    /// 参加ボタンのクリックイベント
    /// </summary>
    public async void OnJoinButtonClick()
    {
        Debug.Log("click join Button! " + this.OwnerNameText.text);

        var holder = ConnectionHolder.Find();
        await holder.Client.EnterGame(this.gameOutline.GameId);
        //await holder.Client.ReadyGame();

        StartCoroutine(LoadYourAsyncScene());
    }

    private IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        var asyncLoad = SceneManager.LoadSceneAsync(SceneNames.BattleScene.ToString());

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
