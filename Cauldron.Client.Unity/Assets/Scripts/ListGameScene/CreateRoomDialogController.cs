using Assets.Scripts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomDialogController : MonoBehaviour
{
    [SerializeField]
    private GameObject deckListView = default;

    [SerializeField]
    private GameObject listNodePrefab = default;

    [SerializeField]
    private TextMeshProUGUI titleText = default;

    [SerializeField]
    private Image titleBackground = default;

    [SerializeField]
    private TMP_InputField messageInputField = default;

    [SerializeField]
    private Button okButton = default;

    [SerializeField]
    private Color youColor = default;
    [SerializeField]
    private Color opponentColor = default;

    private Transform deckListContent;
    private ListDeckNodeController selectedNode;

    private Action<IDeck, string> onOkButtonClickAction;
    private Action onCancelButtonClickAction;

    // Start is called before the first frame update
    void Start()
    {
        this.deckListContent = this.deckListView.transform.Find("Viewport").transform.Find("Content");

        var decks = new DeckRepository().GetAll();

        foreach (var deck in decks)
        {
            this.AddToDeckListView(deck);
        }
    }

    private void AddToDeckListView(IDeck source)
    {
        var node = Instantiate(this.listNodePrefab, this.deckListContent.transform);
        var controller = node.GetComponent<ListDeckNodeController>();
        controller.Set(source, this.SelectNode);
    }

    public void OnOkButtonClick()
    {
        this.gameObject.SetActive(false);
        this.onOkButtonClickAction?.Invoke(
            this.selectedNode.Source,
            this.messageInputField.text
            );
    }

    public void OnCancelButtonClick()
    {
        this.gameObject.SetActive(false);
        this.onCancelButtonClickAction?.Invoke();
    }

    private void SelectNode(ListDeckNodeController nodeController)
    {
        if (this.selectedNode != null)
        {
            this.selectedNode.SetDeselectedColor();
        }

        this.selectedNode = nodeController;
        this.selectedNode.SetSelectedColor();

        this.okButton.interactable = true;
    }

    public void ShowNewRoomDialog(Action<IDeck, string> onOkButtonClickAction, Action onCancelButtonClickAction = null)
    {
        this.ShowDialog(true, true, onOkButtonClickAction, onCancelButtonClickAction);
    }

    public void ShowYouJoinRoomDialog(Action<IDeck, string> onOkButtonClickAction, Action onCancelButtonClickAction = null)
    {
        this.ShowDialog(true, false, onOkButtonClickAction, onCancelButtonClickAction);
    }

    public void ShowAiJoinRoomDialog(Action<IDeck, string> onOkButtonClickAction, Action onCancelButtonClickAction = null)
    {
        this.ShowDialog(false, false, onOkButtonClickAction, onCancelButtonClickAction);
    }

    private void ShowDialog(bool isYou, bool isNew, Action<IDeck, string> onOkButtonClickAction, Action onCancelButtonClickAction = null)
    {
        this.titleText.text = isYou ? "Select Your Deck" : "Select AI Deck";
        this.titleBackground.color = isYou ? this.youColor : this.opponentColor;
        this.onOkButtonClickAction = onOkButtonClickAction;
        this.onCancelButtonClickAction = onCancelButtonClickAction;

        if (this.selectedNode != null)
        {
            this.selectedNode.SetDeselectedColor();
            this.selectedNode = null;
        }

        this.okButton.interactable = false;

        this.messageInputField.gameObject.SetActive(isNew);

        this.gameObject.SetActive(true);
    }
}
