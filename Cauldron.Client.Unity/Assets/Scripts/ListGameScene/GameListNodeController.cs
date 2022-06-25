using Assets.Scripts.ServerShared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;

public class GameListNodeController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ownerNameText = default;
    [SerializeField]
    private TextMeshProUGUI messageText = default;

    private Action onJoinButtonClickAction;

    private RoomOutline roomOutline;

    public void Set(RoomOutline roomOutline, Action onJoinButtonClickAction)
    {
        this.roomOutline = roomOutline;
        this.ownerNameText.text = this.roomOutline.OwnerName;
        this.messageText.text = this.roomOutline.Message;
        this.onJoinButtonClickAction = onJoinButtonClickAction;
    }

    /// <summary>
    /// 参加ボタンのクリックイベント
    /// </summary>
    public void OnJoinButtonClick()
    {
        Debug.Log("click join Button! " + this.ownerNameText.text);

        this.onJoinButtonClickAction?.Invoke();
    }
}
