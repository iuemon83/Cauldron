using Assets.Scripts;
using System;
using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI errorMessageText;
    [SerializeField]
    private InputField ipOrHostNameText;
    [SerializeField]
    private InputField playerNameText;
    [SerializeField]
    private Button startButton;

    private Text startButtonText;

    private void Start()
    {
        this.startButtonText = this.startButton.GetComponentInChildren<Text>();

        this.ipOrHostNameText.text = LocalData.ServerAddress;
        this.playerNameText.text = LocalData.PlayerName;
    }

    /// <summary>
    /// ターン終了ボタンのクイックイベント
    /// </summary>
    public async void OnStartButtonClick()
    {
        this.startButton.interactable = false;
        this.startButtonText.text = "Loading...";

        var isValid = await this.DoValidation();
        if (!isValid)
        {
            this.startButton.interactable = true;
            this.startButtonText.text = "Start";

            return;
        }

        LocalData.ServerAddress = this.ipOrHostNameText.text;
        LocalData.PlayerName = this.playerNameText.text;

        StartCoroutine(LoadYourAsyncScene());
    }

    private IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        var asyncLoad = SceneManager.LoadSceneAsync(SceneNames.ListGameScene.ToString());

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    private async Task<bool> DoValidation()
    {
        if (string.IsNullOrWhiteSpace(this.playerNameText.text))
        {
            this.ShowErrorMessage("プレイヤー名を入力してください");
            return false;
        }

        try
        {
            await ConnectionHolder.Create(this.ipOrHostNameText.text, this.playerNameText.text);
        }
        catch (Exception e)
        {
            // サーバーへの接続に失敗
            this.ShowErrorMessage("サーバーへの接続に失敗しました");
            Debug.LogWarning(e);
            return false;
        }

        return true;
    }

    private void ShowErrorMessage(string message)
    {
        this.errorMessageText.text = message;
    }
}
