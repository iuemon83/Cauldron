using Assets.Scripts;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditDeckSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPoolList;
    [SerializeField]
    private GameObject deckList;
    [SerializeField]
    private GameObject listNodePrefab;
    [SerializeField]
    private InputField deckNameInputField;
    [SerializeField]
    private TextMeshProUGUI deckCountText;
    [SerializeField]
    private CardDefDetailController cardDefDetailController;

    public IDeck DeckToEdit { get; set; }

    private Transform cardPoolListContent;
    private Transform deckListContent;

    private readonly Dictionary<CardDefId, EditDeckScene_ListNodeController> cardPoolListByDefId = new Dictionary<CardDefId, EditDeckScene_ListNodeController>();
    private readonly Dictionary<CardDefId, EditDeckScene_ListNodeController> deckListByDefId = new Dictionary<CardDefId, EditDeckScene_ListNodeController>();

    private int currentDeckTotalCount;

    private bool IsLimitTotalNum => this.currentDeckTotalCount == this.ruleBook.MaxNumDeckCards;

    private RuleBook ruleBook;

    // Start is called before the first frame update
    async void Start()
    {
        this.cardPoolListContent = this.cardPoolList.transform.Find("Viewport").transform.Find("Content");
        this.deckListContent = this.deckList.transform.Find("Viewport").transform.Find("Content");

        var holder = ConnectionHolder.Find();
        this.ruleBook = await holder.Client.GetRuleBook();
        var allCards = await holder.Client.GetCardPool();

        foreach (var card in allCards.OrderBy(c => c.Cost).ThenBy(c => c.FullName))
        {
            this.AddToCardPool(card);
        }

        if (this.DeckToEdit != null)
        {
            this.ApplyDeck(this.DeckToEdit, allCards);
        }

        this.UpdateDeckTotalCountText();
    }

    private void ApplyDeck(IDeck deck, CardDef[] cardPool)
    {
        this.deckNameInputField.text = deck.Name;

        foreach (var cardName in deck.CardDefNames)
        {
            var cardDef = cardPool.FirstOrDefault(cd => cd.FullName == cardName);
            if (cardDef != null)
            {
                this.AddToDeck(cardDef.Id);
            }
        }
    }

    private bool IsValidNumCards() => this.currentDeckTotalCount >= this.ruleBook.MinNumDeckCards
        && this.currentDeckTotalCount <= this.ruleBook.MaxNumDeckCards;

    public void OnSaveButtonClick()
    {
        var deckName = this.deckNameInputField.text;
        if (string.IsNullOrWhiteSpace(deckName))
        {
            Debug.LogWarning("デッキ名が未入力");
            return;
        }

        if (!this.IsValidNumCards())
        {
            Debug.LogWarning("デッキ枚数が不正");
            return;
        }

        var cardDefList = this.deckListByDefId.Values
            .SelectMany(con => Enumerable.Repeat(con.Source, con.CurrentDeckCount));

        if (this.DeckToEdit == null)
        {
            new DeckRepository().Add(deckName, cardDefList);
        }
        else
        {
            new DeckRepository().Update(this.DeckToEdit.Id, deckName, cardDefList);
        }

        Utility.LoadAsyncScene(this, SceneNames.ListDeckScene);
    }

    public void OnCancelButtonClick()
    {
        Utility.LoadAsyncScene(this, SceneNames.ListDeckScene);
    }

    private void AddToCardPool(CardDef cardDef)
    {
        var node = Instantiate(this.listNodePrefab, this.cardPoolListContent.transform);
        var controller = node.GetComponent<EditDeckScene_ListNodeController>();
        controller.Init(cardDef, 3,
            () => this.AddToDeck(cardDef.Id),
            () => this.RemoveFromDeck(cardDef.Id),
            this.cardDefDetailController.SetCard
            );

        this.cardPoolListByDefId.Add(cardDef.Id, controller);
    }

    private void AddToDeck(CardDefId cardDefId)
    {
        if (this.IsLimitTotalNum)
        {
            // デッキ上限
            return;
        }

        var cardPoolListNodeController = this.cardPoolListByDefId[cardDefId];

        if (cardPoolListNodeController.IsLimitNumByCard)
        {
            // カードごとの上限
            return;
        }

        if (!this.deckListByDefId.TryGetValue(cardDefId, out var deckListNodeController))
        {
            // 指定したid が存在しない
            var node = Instantiate(this.listNodePrefab, this.deckListContent.transform);
            deckListNodeController = node.GetComponent<EditDeckScene_ListNodeController>();
            deckListNodeController.Init(cardPoolListNodeController.Source,
                cardPoolListNodeController.LimitDeckCount,
                () => this.AddToDeck(cardDefId),
                () => this.RemoveFromDeck(cardDefId),
                this.cardDefDetailController.SetCard
                );

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
        this.deckCountText.text = $"{this.currentDeckTotalCount} / {this.ruleBook.MaxNumDeckCards}";
    }
}
