using Assets.Scripts.ServerShared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;

public class GameListNodeController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI OwnerNameText;

    private Action onJoinButtonClickAction;

    private GameOutline gameOutline;

    public void Set(GameOutline gameOutline, Action onJoinButtonClickAction)
    {
        this.gameOutline = gameOutline;
        this.OwnerNameText.text = this.gameOutline.OwnerName;
        this.onJoinButtonClickAction = onJoinButtonClickAction;
    }

    /// <summary>
    /// �Q���{�^���̃N���b�N�C�x���g
    /// </summary>
    public void OnJoinButtonClick()
    {
        Debug.Log("click join Button! " + this.OwnerNameText.text);

        this.onJoinButtonClickAction?.Invoke();
    }
}
