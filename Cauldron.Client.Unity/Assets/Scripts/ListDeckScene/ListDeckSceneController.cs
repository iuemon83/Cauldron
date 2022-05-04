using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class ListDeckSceneController : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas = default;

    [SerializeField]
    private GameObject deckListViewNodePrefab = default;

    [SerializeField]
    private ConfirmDialogController confirmDialogController = default;

    [SerializeField]
    private GameObject deckListView = default;
    [SerializeField]
    private Button editButton = default;
    [SerializeField]
    private Button deleteButton = default;
    [SerializeField]
    private AudioSource audioSource = default;

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

    public async void OnNewButtonClick()
    {
        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

        await Utility.LoadAsyncScene(SceneNames.EditDeckScene);
    }

    public async void OnEditButtonClick()
    {
        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

        await Utility.LoadAsyncScene(SceneNames.EditDeckScene, () =>
        {
            var editDeckSceneController = FindObjectOfType<EditDeckSceneController>();
            editDeckSceneController.DeckToEdit = this.selectedNode.Source;
        });
    }

    public void OnDeleteButtonClick()
    {
        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

        // 確認ダイアログ
        var title = "デッキの削除";
        var message = $"「{this.selectedNode.Source.Name}」を削除してもよろしいですか？";
        var dialog = Instantiate(this.confirmDialogController);
        dialog.Init(title, message, ConfirmDialogController.DialogType.Confirm,
            onOkAction: () =>
            {
                new DeckRepository().Delete(this.selectedNode.Source.Id);
                this.RefreshDeckList();
            });
        dialog.transform.SetParent(this.canvas.transform, false);
    }

    public async void OnCloseButtonClick()
    {
        await Utility.LoadAsyncScene(SceneNames.ListGameScene);
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

    private void PlayAudio(SeAudioCache.SeAudioType audioType)
    {
        var (b, a) = SeAudioCache.GetOrInit(audioType);
        if (b)
        {
            this.audioSource.PlayOneShot(a);
        }
    }
}
