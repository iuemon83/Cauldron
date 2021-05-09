using Assets.Scripts;
using TMPro;
using UniRx;
using UnityEngine;

public class WatingRoomSceneController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI loadingText;

    private readonly float updateInterval = 0.5f;
    private float timeLeft = 0f;

    private readonly int countLimit = 3;
    private int count;
    private string baseLoadingText;

    // Start is called before the first frame update
    void Start()
    {
        this.baseLoadingText = this.loadingText.text;

        var holder = ConnectionHolder.Find();
        holder.Receiver.OnJoinGame.Subscribe(_ =>
        {
            Utility.LoadAsyncScene(this, SceneNames.BattleScene);
        });
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
                this.loadingText.text = this.baseLoadingText;
            }
            else
            {
                this.count++;
                this.loadingText.text += ".";
            }
        }
    }
}
