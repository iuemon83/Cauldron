using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialogController : MonoBehaviour
{
    public enum DialogType
    {
        Message,
        Confirm
    }

    [SerializeField]
    private Text titleText;
    [SerializeField]
    private Text messageText;

    [SerializeField]
    private Button cancelButton;

    public Action OnOkButtonClickAction { get; set; }
    public Action OnCancelButtonClickAction { get; set; }

    public void Init(string title, string message, DialogType dialogType)
    {
        this.titleText.text = title;
        this.messageText.text = message;

        switch (dialogType)
        {
            case DialogType.Confirm:
                this.cancelButton.gameObject.SetActive(true);
                break;

            case DialogType.Message:
                this.cancelButton.gameObject.SetActive(false);
                break;
        }
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
