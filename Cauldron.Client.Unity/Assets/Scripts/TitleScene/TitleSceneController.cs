using Assets.Scripts;
using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
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
    /// �^�[���I���{�^���̃N�C�b�N�C�x���g
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

        await Utility.LoadAsyncScene(SceneNames.ListGameScene);
    }

    private async UniTask<bool> DoValidation()
    {
        if (string.IsNullOrWhiteSpace(this.playerNameText.text))
        {
            this.ShowErrorMessage("�v���C���[������͂��Ă�������");
            return false;
        }

        try
        {
            await ConnectionHolder.Create(this.ipOrHostNameText.text, this.playerNameText.text);
        }
        catch (Exception e)
        {
            // �T�[�o�[�ւ̐ڑ��Ɏ��s
            this.ShowErrorMessage("�T�[�o�[�ւ̐ڑ��Ɏ��s���܂���");
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
