using Assets.Scripts;
using Grpc.Core;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    public Text ErrorMessageText;
    public Text IpOrHostNameText;
    public Text PlayerNameText;
    public Button StartButton;
    public Text StartButtonText;

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

        StartCoroutine(LoadYourAsyncScene());
    }

    private IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(SceneNames.BattleScene.ToString());

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
            var channel = new Channel(this.IpOrHostNameText.text, ChannelCredentials.Insecure);

            await channel.ConnectAsync(DateTime.UtcNow.Add(TimeSpan.FromSeconds(5)));

            Config.Channel = channel;
        }
        catch (Exception e)
        {
            // サーバーへの接続に失敗
            this.ShowErrorMessage("サーバーへの接続に失敗しました");
            Debug.LogError(e);
            return false;
        }

        Config.ServerAddress = this.IpOrHostNameText.text;
        Config.PlayerName = this.PlayerNameText.text;

        return true;
    }

    private void ShowErrorMessage(string message)
    {
        this.ErrorMessageText.text = message;
    }
}
