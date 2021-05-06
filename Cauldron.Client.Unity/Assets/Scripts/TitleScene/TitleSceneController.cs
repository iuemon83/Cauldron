using Assets.Scripts;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField]
    private Text ErrorMessageText;
    [SerializeField]
    private InputField IpOrHostNameText;
    [SerializeField]
    private InputField PlayerNameText;
    [SerializeField]
    private Button StartButton;

    private Text StartButtonText;

    private void Start()
    {
        this.StartButtonText = this.StartButton.GetComponentInChildren<Text>();

        this.IpOrHostNameText.text = LocalData.ServerAddress;
        this.PlayerNameText.text = LocalData.PlayerName;
    }

    /// <summary>
    /// ターン終了ボタンのクイックイベント
    /// </summary>
    public async void OnStartButtonClick()
    {
        this.StartButton.interactable = false;
        this.StartButtonText.text = "Loading...";

        var isValid = await this.DoValidation();
        if (!isValid)
        {
            this.StartButton.interactable = true;
            this.StartButtonText.text = "Start";

            return;
        }

        LocalData.ServerAddress = this.IpOrHostNameText.text;
        LocalData.PlayerName = this.PlayerNameText.text;

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
        if (string.IsNullOrWhiteSpace(this.PlayerNameText.text))
        {
            this.ShowErrorMessage("プレイヤー名を入力してください");
            return false;
        }

        try
        {
            await ConnectionHolder.Create(this.IpOrHostNameText.text, this.PlayerNameText.text);
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
        this.ErrorMessageText.text = message;
    }
}
