using Assets.Scripts;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TitleSceneController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI errorMessageText = default;
    [SerializeField]
    private InputField ipOrHostNameText = default;
    [SerializeField]
    private InputField playerNameText = default;
    [SerializeField]
    private Button startButton = default;
    [SerializeField]
    private AudioSource audioSource = default;
    [SerializeField]
    private TextMeshProUGUI versionText = default;

    private Text startButtonText;

    private void Start()
    {
        this.startButtonText = this.startButton.GetComponentInChildren<Text>();
        this.versionText.text = $"ver.{Config.Version}";

        this.ipOrHostNameText.text = LocalData.ServerAddress;
        this.playerNameText.text = LocalData.PlayerName;
    }

    /// <summary>
    /// ターン終了ボタンのクイックイベント
    /// </summary>
    public async void OnStartButtonClick()
    {
        this.startButton.interactable = false;

        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

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

        await Utility.LoadAsyncScene(SceneNames.ListGameScene);
    }

    public async void OnLicenseButtonClick()
    {
        await Utility.LoadAsyncScene(SceneNames.DisplayLicenseScene);
    }

    private async UniTask<bool> DoValidation()
    {
        if (string.IsNullOrWhiteSpace(this.playerNameText.text))
        {
            this.ShowErrorMessage("プレイヤー名を入力してください");
            return false;
        }

        try
        {
            var holder = await ConnectionHolder.Create(this.ipOrHostNameText.text, this.playerNameText.text);
            await holder.LoadCardPool();
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

    private void PlayAudio(SeAudioCache.SeAudioType audioType)
    {
        var (b, a) = SeAudioCache.GetOrInit(audioType);
        if (b)
        {
            this.audioSource.PlayOneShot(a);
        }
    }
}
