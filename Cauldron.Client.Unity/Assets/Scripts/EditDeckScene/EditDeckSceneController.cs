using Assets.Scripts;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EditDeckSceneController : MonoBehaviour
{
    public GameObject CardPoolList;
    public GameObject DeckList;
    public GameObject ListNodePrefab;
    public InputField DeckNameInputField;
    public Text DeckCountText;
    public CardDefDetailController CardDefDetailController;

    private Transform cardPoolListContent;
    private Transform deckListContent;

    private readonly Dictionary<CardDefId, ListNodeController> cardPoolListByDefId = new Dictionary<CardDefId, ListNodeController>();
    private readonly Dictionary<CardDefId, ListNodeController> deckListByDefId = new Dictionary<CardDefId, ListNodeController>();

    private int currentDeckTotalCount;
    private int LimitDeckTotalCount;

    private bool IsLimit => this.currentDeckTotalCount == this.LimitDeckTotalCount;

    // Start is called before the first frame update
    async void Start()
    {
        // test code
        var holder1 = await ConnectionHolder.Create("localhost:5000", "test player");
        await holder1.Client.OpenNewGame();

        this.cardPoolListContent = this.CardPoolList.transform.Find("Viewport").transform.Find("Content");
        this.deckListContent = this.DeckList.transform.Find("Viewport").transform.Find("Content");

        this.LimitDeckTotalCount = 5;

        var holder = ConnectionHolder.Find();
        var cards = await holder.Client.GetCardPool();

        foreach (var card in cards.Cards.OrderBy(c => c.Cost).ThenBy(c => c.FullName))
        {
            this.AddToCardPool(card);
        }
    }

    public void OnSaveButtonClick()
    {
        var deckName = this.DeckNameInputField.text;
        if (string.IsNullOrWhiteSpace(deckName))
        {
            Debug.LogWarning("デッキ名が未入力");
            return;
        }

        if (!this.IsLimit)
        {
            Debug.LogWarning("デッキ枚数が不足");
            return;
        }

        var cardDefList = this.deckListByDefId.Values
            .SelectMany(con => Enumerable.Repeat(con.Source, con.CurrentDeckCount));
        new DeckRepository().Add(deckName, cardDefList);

        //StartCoroutine(Utility.LoadAsyncSceneCoroutine(SceneNames.ListGameScene));
    }

    public void OnCancelButtonClick()
    {
        //StartCoroutine(Utility.LoadAsyncSceneCoroutine(SceneNames.ListGameScene));
    }

    private void AddToCardPool(CardDef cardDef)
    {
        var node = Instantiate(this.ListNodePrefab, this.cardPoolListContent.transform);
        var controller = node.GetComponent<ListNodeController>();
        controller.Set(cardDef, 3);
        controller.AddToDeckAction = () => this.AddToDeck(cardDef.Id);
        controller.RemoveFromDeckAction = () => this.RemoveFromDeck(cardDef.Id);
        controller.ShowDetailAction = this.CardDefDetailController.SetCard;

        this.cardPoolListByDefId.Add(cardDef.Id, controller);
    }

    private void AddToDeck(CardDefId cardDefId)
    {
        var cardPoolListNodeController = this.cardPoolListByDefId[cardDefId];

        if (this.IsLimit)
        {
            // デッキ上限
            return;
        }

        if (cardPoolListNodeController.IsLimit)
        {
            // デッキ上限
            return;
        }

        if (!this.deckListByDefId.TryGetValue(cardDefId, out var deckListNodeController))
        {
            // 指定したid が存在しない
            var node = Instantiate(this.ListNodePrefab, this.deckListContent.transform);
            deckListNodeController = node.GetComponent<ListNodeController>();
            deckListNodeController.Set(cardPoolListNodeController.Source, cardPoolListNodeController.LimitDeckCount);
            deckListNodeController.AddToDeckAction = () => this.AddToDeck(cardDefId);
            deckListNodeController.RemoveFromDeckAction = () => this.RemoveFromDeck(cardDefId);
            deckListNodeController.ShowDetailAction = this.CardDefDetailController.SetCard;

            this.deckListByDefId.Add(cardDefId, deckListNodeController);

            this.SortDeckListNodes();
        }

        deckListNodeController.AddOne();
        cardPoolListNodeController.AddOne();

        this.currentDeckTotalCount += 1;
        this.UpdateDeckTotalCountText();
    }

    private void SortDeckListNodes()
    {
        var sorted = this.deckListByDefId.Values
            .OrderBy(c => c.Source.Cost)
            .ThenBy(c => c.Source.FullName);

        var index = 0;
        foreach (var transform in sorted.Select(c => c.gameObject.transform))
        {
            transform.SetSiblingIndex(index);
            index++;
        }
    }

    private void RemoveFromDeck(CardDefId cardDefId)
    {
        var cardPoolListNodeController = this.cardPoolListByDefId[cardDefId];

        if (!this.deckListByDefId.TryGetValue(cardDefId, out var deckListNodeController))
        {
            // 指定したid が存在しない
            return;
        }

        deckListNodeController.RemoveOne();
        if (deckListNodeController.IsEmpty)
        {
            // デッキへの投入枚数がゼロ
            Destroy(deckListNodeController.gameObject);
            this.deckListByDefId.Remove(cardDefId);
        }

        cardPoolListNodeController.RemoveOne();

        this.currentDeckTotalCount -= 1;
        this.UpdateDeckTotalCountText();
    }

    private void UpdateDeckTotalCountText()
    {
        this.DeckCountText.text = $"{this.currentDeckTotalCount} / {this.LimitDeckTotalCount}";
    }
}
