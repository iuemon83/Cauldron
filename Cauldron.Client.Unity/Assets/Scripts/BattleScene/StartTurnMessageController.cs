using Assets.Scripts;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class StartTurnMessageController : MonoBehaviour
{
    private static readonly float messageMoveTime = 0.5f;
    private static readonly int messageStopTime = 500;

    [SerializeField]
    private StartTurnMessageContainerController messageContainer1 = default;
    [SerializeField]
    private StartTurnMessageContainerController messageContainer2 = default;

    public async UniTask Show(bool isYou, bool isStartGame)
    {
        if (isStartGame)
        {
            this.messageContainer1.AsStartGame();

            if (isYou)
            {
                this.messageContainer2.AsYou();
            }
            else
            {
                this.messageContainer2.AsOpponent();
            }
        }
        else
        {
            if (isYou)
            {
                this.messageContainer1.AsOpponent();
                this.messageContainer2.AsYou();
            }
            else
            {
                this.messageContainer1.AsYou();
                this.messageContainer2.AsOpponent();
            }
        }

        this.messageContainer1.transform.localPosition = Vector3.zero;

        var w = this.messageContainer2.Width;

        this.messageContainer2.transform.localPosition = new Vector3(-w, 0);

        this.gameObject.SetActive(true);

        AudioController.CreateOrFind().PlayAudio(SeAudioCache.SeAudioType.EndTurn);

        await UniTask.Delay(messageStopTime);

        var t1 = this.messageContainer1.transform
            .DOLocalMoveX(w, messageMoveTime)
            .ToUniTask();

        var t2 = this.messageContainer2.transform
            .DOLocalMoveX(0, messageMoveTime)
            .ToUniTask();

        await UniTask.WhenAll(new[] { t1, t2 });

        AudioController.CreateOrFind().PlayAudio(SeAudioCache.SeAudioType.StartTurn);

        await UniTask.Delay(messageStopTime);

        this.gameObject.SetActive(false);
    }
}
