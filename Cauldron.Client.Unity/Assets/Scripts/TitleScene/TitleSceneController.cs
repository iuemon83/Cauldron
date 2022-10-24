using Assets.Scripts;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CardAudioCache;

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
    private TextMeshProUGUI versionText = default;

    private Text startButtonText;

    private void Start()
    {
        this.startButtonText = this.startButton.GetComponentInChildren<Text>();
        this.versionText.text = $"ver.{Application.version}";

        this.ipOrHostNameText.text = LocalData.ServerAddress;
        this.playerNameText.text = LocalData.PlayerName;
    }

    /// <summary>
    /// �^�[���I���{�^���̃N�C�b�N�C�x���g
    /// </summary>
    public async void OnStartButtonClick()
    {
        this.startButton.interactable = false;

        AudioController.CreateOrFind().PlayAudio(SeAudioCache.SeAudioType.Ok);

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
            this.ShowErrorMessage("�v���C���[������͂��Ă�������");
            return false;
        }

        try
        {
            var holder = await ConnectionHolder.Create(this.ipOrHostNameText.text, this.playerNameText.text);

            var reply = await holder.Client.ListAllowedClientVersions();
            if (!reply?.AllowedClientVersions.Contains(Application.version) ?? false)
            {
                this.ShowErrorMessage(@$"�V�����o�[�W�����������[�X����Ă��܂��B
�L���ȃo�[�W������{string.Join(",", reply.AllowedClientVersions)}�ł��B");
                return false;
            }

            await holder.LoadCardPool();

            // �S�J�[�h�f�[�^�̓ǂݍ���
            foreach (var name in holder.CardPool.Values.Select(c => c.Name))
            {
                CardImageCache.GetOrInit(name);
            }
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
