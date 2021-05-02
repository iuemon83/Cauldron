using Assets.Scripts;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WatingRoomSceneController : MonoBehaviour
{
    public Text LoadingText;

    private readonly float updateInterval = 0.5f;
    private float timeLeft = 0f;

    private int countLimit = 3;
    private int count;
    private string baseLoadingText;

    // Start is called before the first frame update
    void Start()
    {
        this.baseLoadingText = this.LoadingText.text;

        var holder = ConnectionHolder.Find();
        holder.Receiver.OnJoinGame.Subscribe(_ =>
        {
            StartCoroutine(LoadYourAsyncScene());
        });
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

    // Update is called once per frame
    void Update()
    {
        timeLeft += Time.deltaTime;
        if (timeLeft > updateInterval)
        {
            timeLeft = 0f;

            if (this.count == this.countLimit)
            {
                this.count = 0;
                this.LoadingText.text = this.baseLoadingText;
            }
            else
            {
                this.count++;
                this.LoadingText.text += ".";
            }
        }
    }
}
