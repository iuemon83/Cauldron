using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class ListDeckSceneController : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private GameObject deckListViewNodePrefab;

    [SerializeField]
    private ConfirmDialogController confirmDialogController;

    [SerializeField]
    private GameObject deckListView;
    [SerializeField]
    private Button newButton;
    [SerializeField]
    private Button editButton;
    [SerializeField]
    private Button deleteButton;
    [SerializeField]
    private Button closeButton;

    private Transform deckListViewContent;
    private ListDeckScene_ListDeckNodeController selectedNode;

    // Start is called before the first frame update
    void Start()
    {
        this.deckListViewContent = this.deckListView.transform.Find("Viewport").transform.Find("Content");

        this.editButton.interactable = false;
        this.deleteButton.interactable = false;

        this.RefreshDeckList();
    }

    private void RefreshDeckList()
    {
        // clear list
        foreach (Transform child in this.deckListViewContent.transform)
        {
            Destroy(child.gameObject);
        }

        var decks = new DeckRepository().GetAll();
        foreach (var deck in decks)
        {
            this.AddToDeckListView(deck);
        }
    }

    public void OnNewButtonClick()
    {
        Utility.LoadAsyncScene(this, SceneNames.EditDeckScene);
    }

    public void OnEditButtonClick()
    {
        Utility.LoadAsyncScene(this, SceneNames.EditDeckScene, () =>
        {
            var editDeckSceneController = FindObjectOfType<EditDeckSceneController>();
            editDeckSceneController.DeckToEdit = this.selectedNode.Source;
        });
    }

    public void OnDeleteButtonClick()
    {
        // 確認ダイアログ
        var title = "デッキの削除";
        var message = $"「{this.selectedNode.Source.Name}」を削除してもよろしいですか？";
        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.Confirm);
        dialog.OnOkButtonClickAction = () =>
        {
            new DeckRepository().Delete(this.selectedNode.Source.Id);
            this.RefreshDeckList();
        };
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    public void OnCloseButtonClick()
    {
        Utility.LoadAsyncScene(this, SceneNames.ListGameScene);
    }

    private void AddToDeckListView(IDeck source)
    {
        var node = Instantiate(this.deckListViewNodePrefab, this.deckListViewContent.transform);
        var controller = node.GetComponent<ListDeckScene_ListDeckNodeController>();
        controller.SelectNodeAction = this.SelectNode;
        controller.Set(source);
    }

    private void SelectNode(ListDeckScene_ListDeckNodeController nodeController)
    {
        if (this.selectedNode != null)
        {
            this.selectedNode.SetDeselectedColor();
        }

        this.selectedNode = nodeController;
        this.selectedNode.SetSelectedColor();

        this.editButton.interactable = true;
        this.deleteButton.interactable = true;
    }
}
