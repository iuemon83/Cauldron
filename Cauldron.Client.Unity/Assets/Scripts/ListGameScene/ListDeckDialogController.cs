using Assets.Scripts;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ListDeckDialogController : MonoBehaviour
{
    [SerializeField]
    private GameObject DeckListView;

    [SerializeField]
    private GameObject ListNodePrefab;

    [SerializeField]
    private Button OkButton;

    private Transform deckListContent;
    private ListDeckNodeController selectedNode;


    public Action<IDeck> OnOkButtonClickAction { get; set; }
    public Action OnCancelButtonClickAction { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        this.deckListContent = this.DeckListView.transform.Find("Viewport").transform.Find("Content");

        var decks = new DeckRepository().GetAll();

        foreach (var deck in decks)
        {
            this.AddToDeckListView(deck);
        }
    }

    private void AddToDeckListView(IDeck source)
    {
        var node = Instantiate(this.ListNodePrefab, this.deckListContent.transform);
        var controller = node.GetComponent<ListDeckNodeController>();
        controller.SelectNodeAction = this.SelectNode;
        controller.Set(source);
    }

    public void OnOkButtonClick()
    {
        this.gameObject.SetActive(false);
        this.OnOkButtonClickAction?.Invoke(this.selectedNode.Source);
    }

    public void OnCancelButtonClick()
    {
        this.gameObject.SetActive(false);
        this.OnCancelButtonClickAction?.Invoke();
    }

    private void SelectNode(ListDeckNodeController nodeController)
    {
        if (this.selectedNode != null)
        {
            this.selectedNode.SetDeselectedColor();
        }

        this.selectedNode = nodeController;
        this.selectedNode.SetSelectedColor();

        this.OkButton.interactable = true;
    }

    public void ShowDialog()
    {
        if (this.selectedNode != null)
        {
            this.selectedNode.SetDeselectedColor();
            this.selectedNode = null;
        }

        this.OkButton.interactable = false;

        this.gameObject.SetActive(true);
    }
}
