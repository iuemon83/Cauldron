using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialogController : MonoBehaviour
{
    public enum DialogType
    {
        Message,
        Confirm,
        OnlyCancel
    }

    [SerializeField]
    private Text titleText;
    [SerializeField]
    private Text messageText;

    [SerializeField]
    private Button okButton;
    [SerializeField]
    private Button cancelButton;

    public Action OnOkButtonClickAction { get; private set; }
    public Action OnCancelButtonClickAction { get; private set; }

    public void Init(string title, string message, DialogType dialogType,
        Action onOkAction = null, Action onCancelAction = null)
    {
        this.titleText.text = title;
        this.messageText.text = message;

        switch (dialogType)
        {
            case DialogType.Confirm:
                this.okButton.gameObject.SetActive(true);
                this.cancelButton.gameObject.SetActive(true);

                this.OnOkButtonClickAction = onOkAction;
                this.OnCancelButtonClickAction = onCancelAction;

                break;

            case DialogType.Message:
                this.okButton.gameObject.SetActive(true);
                this.cancelButton.gameObject.SetActive(false);

                this.OnOkButtonClickAction = onOkAction;

                // マスクのクリックでキャンセルが動くからこっちにも入れないとダメ
                this.OnCancelButtonClickAction = onOkAction;

                break;

            case DialogType.OnlyCancel:
                this.okButton.gameObject.SetActive(false);
                this.cancelButton.gameObject.SetActive(true);

                this.OnOkButtonClickAction = onCancelAction;
                this.OnCancelButtonClickAction = onCancelAction;

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
