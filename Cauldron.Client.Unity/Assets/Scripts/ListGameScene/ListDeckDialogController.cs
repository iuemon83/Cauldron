using Assets.Scripts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ListDeckDialogController : MonoBehaviour
{
    [SerializeField]
    private GameObject deckListView;

    [SerializeField]
    private GameObject listNodePrefab;

    [SerializeField]
    private TextMeshProUGUI titleText;

    [SerializeField]
    private Button okButton;

    private Transform deckListContent;
    private ListDeckNodeController selectedNode;

    private Action<IDeck> onOkButtonClickAction;
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
        this.onOkButtonClickAction?.Invoke(this.selectedNode.Source);
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

    public void ShowDialog(string title, Action<IDeck> onOkButtonClickAction, Action onCancelButtonClickAction = null)
    {
        this.titleText.text = title;
        this.onOkButtonClickAction = onOkButtonClickAction;
        this.onCancelButtonClickAction = onCancelButtonClickAction;

        if (this.selectedNode != null)
        {
            this.selectedNode.SetDeselectedColor();
            this.selectedNode = null;
        }

        this.okButton.interactable = false;

        this.gameObject.SetActive(true);
    }
}
