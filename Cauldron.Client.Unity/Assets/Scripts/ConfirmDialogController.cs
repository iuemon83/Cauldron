using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialogController : MonoBehaviour
{
    [SerializeField]
    private Text titleText;
    [SerializeField]
    private Text messageText;

    public Action OnOkButtonClickAction { get; set; }
    public Action OnCancelButtonClickAction { get; set; }

    public void Init(string title, string message)
    {
        this.titleText.text = title;
        this.messageText.text = message;
    }

    public void OnOkButtonClick()
    {
        this.gameObject.SetActive(false);
        this.OnOkButtonClickAction?.Invoke();
    }

    public void OnCancelButtonClick()
    {
        this.gameObject.SetActive(false);
        this.OnCancelButtonClickAction?.Invoke();
    }
}
