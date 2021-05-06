using Assets.Scripts.ServerShared.MessagePackObjects;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameListNodeController : MonoBehaviour
{
    public Text OwnerNameText;

    public Action OnJoinButtonClickAction;

    private GameOutline gameOutline;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Set(GameOutline gameOutline)
    {
        this.gameOutline = gameOutline;
        this.OwnerNameText.text = this.gameOutline.OwnerName;
    }

    /// <summary>
    /// 参加ボタンのクリックイベント
    /// </summary>
    public void OnJoinButtonClick()
    {
        Debug.Log("click join Button! " + this.OwnerNameText.text);

        this.OnJoinButtonClickAction?.Invoke();
    }
}
