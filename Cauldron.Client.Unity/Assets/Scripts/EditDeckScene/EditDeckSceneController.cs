using Assets.Scripts;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditDeckSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPoolList = default;
    [SerializeField]
    private GameObject deckList = default;
    [SerializeField]
    private GameObject listNodePrefab = default;
    [SerializeField]
    private InputField deckNameInputField = default;
    [SerializeField]
    private TextMeshProUGUI deckCountText = default;
    [SerializeField]
    private CardDefDetailController cardDefDetailController = default;
    [SerializeField]
    private InputField searchKeywordInputField = default;

    public IDeck DeckToEdit { get; set; }

    private Transform cardPoolListContent;
    private Transform deckListContent;

    private readonly Dictionary<CardDefId, EditDeckScene_ListNodeController> cardPoolListByDefId
        = new Dictionary<CardDefId, EditDeckScene_ListNodeController>();
    private readonly Dictionary<CardDefId, EditDeckScene_ListNodeController> deckListByDefId
        = new Dictionary<CardDefId, EditDeckScene_ListNodeController>();

    private int currentDeckTotalCount;

    private bool IsLimitTotalNum => this.currentDeckTotalCount == this.ruleBook.MaxNumDeckCards;

    private RuleBook ruleBook;

    private CardDef[] allCards;

    private Dictionary<CardDefId, int> numCardsLimitListByCardDefId
        = new Dictionary<CardDefId, int>();

    // Start is called before the first frame update
    async void Start()
    {
        this.cardPoolListContent = this.cardPoolList.transform.Find("Viewport").transform.Find("Content");
        this.deckListContent = this.deckList.transform.Find("Viewport").transform.Find("Content");

        var holder = ConnectionHolder.Find();
        this.ruleBook = await holder.Client.GetRuleBook();
        this.allCards = holder.CardPool.Values
            .OrderBy(c => c.Cost)
            .ThenBy(c => c.FullName)
            .ToArray();

        this.numCardsLimitListByCardDefId = this.allCards.ToDictionary(c => c.Id, c => c.IsToken ? 0 : 3);

        foreach (var card in this.allCards)
        {
            this.AddToCardPool(card);
        }

        if (this.DeckToEdit != null)
        {
            this.ApplyDeck(this.DeckToEdit, this.allCards);
        }

        this.UpdateDeckTotalCountText();
    }

    public void OnSearchButtonClick()
    {
        var keyword = this.searchKeywordInputField.text;
        this.SearchCardPool(keyword);
    }

    private void SearchCardPool(string keyword)
    {
        foreach (var listNode in this.cardPoolListByDefId.Values)
        {
            Destroy(listNode.gameObject);
        }
        this.cardPoolListByDefId.Clear();

        var matchedCards = this.allCards.Where(c => this.IsMatchedByKeyword(keyword, c));
        foreach (var c in matchedCards)
        {
            this.AddToCardPool(c);
        }
    }

    private bool IsMatchedByKeyword(string keyword, CardDef cardDef)
    {
        return cardDef.Name.Contains(keyword)
            || cardDef.EffectDescription.Contains(keyword)
            || cardDef.Annotations.Contains(keyword)
            || cardDef.FlavorText.Contains(keyword)
            ;
    }

    /// <summary>
    /// �쐬�ς݂̃f�b�L����ʂɔ��f����
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="cardPool"></param>
    private void ApplyDeck(IDeck deck, CardDef[] cardPool)
    {
        this.deckNameInputField.text = deck.Name;

        foreach (var cardName in deck.CardDefNames)
        {
            var cardDef = cardPool.FirstOrDefault(cd => cd.FullName == cardName);
            if (cardDef != null)
            {
                this.AddToDeck(cardDef);
            }
        }
    }

    private bool IsValidNumCards() => this.currentDeckTotalCount >= this.ruleBook.MinNumDeckCards
        && this.currentDeckTotalCount <= this.ruleBook.MaxNumDeckCards;

    public async void OnSaveButtonClick()
    {
        var deckName = this.deckNameInputField.text;
        if (string.IsNullOrWhiteSpace(deckName))
        {
            Debug.LogWarning("�f�b�L����������");
            return;
        }

        if (!this.IsValidNumCards())
        {
            Debug.LogWarning("�f�b�L�������s��");
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

        await Utility.LoadAsyncScene(SceneNames.ListDeckScene);
    }

    public async void OnCancelButtonClick()
    {
        await Utility.LoadAsyncScene(SceneNames.ListDeckScene);
    }

    /// <summary>
    /// �J�[�h�v�[���̈ꗗ�ɃJ�[�h��ǉ�����
    /// </summary>
    /// <param name="cardDef"></param>
    private void AddToCardPool(CardDef cardDef)
    {
        if (!this.numCardsLimitListByCardDefId.TryGetValue(cardDef.Id, out var limit))
        {
            Debug.Log($"�w�肳�ꂽID�����݂��Ȃ��BCardDefId={cardDef.Id}");
            return;
        }

        var currentNumCards = this.deckListByDefId.TryGetValue(cardDef.Id, out var deckListNodeController)
            ? deckListNodeController.CurrentDeckCount
            : 0;

        var node = Instantiate(this.listNodePrefab, this.cardPoolListContent.transform);
        var controller = node.GetComponent<EditDeckScene_ListNodeController>();
        controller.Init(
            cardDef,
            limit,
            currentNumCards,
            () => this.AddToDeck(cardDef),
            () => this.RemoveFromDeck(cardDef.Id),
            this.cardDefDetailController.SetCard
            );

        this.cardPoolListByDefId.Add(cardDef.Id, controller);
    }

    /// <summary>
    /// �f�b�L��ʑ��ɃJ�[�h��ǉ�����
    /// </summary>
    /// <param name="cardDefId"></param>
    private void AddToDeck(CardDef cardDef)
    {
        if (this.IsLimitTotalNum)
        {
            Debug.Log("�f�b�L���");
            return;
        }

        if (!this.numCardsLimitListByCardDefId.TryGetValue(cardDef.Id, out var limit))
        {
            Debug.Log($"�w�肳�ꂽID�����݂��Ȃ��BCardDefId={cardDef.Id}");
            return;
        }

        if (limit == 0)
        {
            Debug.Log("�f�b�L�ɓ����ł��閇����0��");
            return;
        }

        if (!this.deckListByDefId.TryGetValue(cardDef.Id, out var deckListNodeController))
        {
            // �w�肵��id �����݂��Ȃ�
            //�܂��f�b�L�ɓ�������Ă��Ȃ�
            var node = Instantiate(this.listNodePrefab, this.deckListContent.transform);
            deckListNodeController = node.GetComponent<EditDeckScene_ListNodeController>();
            deckListNodeController.Init(
                cardDef,
                limit,
                0,
                () => this.AddToDeck(cardDef),
                () => this.RemoveFromDeck(cardDef.Id),
                this.cardDefDetailController.SetCard
                );

            this.deckListByDefId.Add(cardDef.Id, deckListNodeController);

            this.SortDeckListNodes();
        }

        if (deckListNodeController.IsLimitNumByCard)
        {
            Debug.Log("�f�b�L�ɓ����ł���������");
            return;
        }

        deckListNodeController.AddOne();

        if (this.cardPoolListByDefId.TryGetValue(cardDef.Id, out var cardPoolListNodeController))
        {
            cardPoolListNodeController.AddOne();
        }

        this.currentDeckTotalCount++;
        this.UpdateDeckTotalCountText();
    }

    /// <summary>
    /// �f�b�L�̈ꗗ���\�[�g����
    /// </summary>
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

    /// <summary>
    /// �f�b�L��ʑ�����J�[�h���폜����
    /// </summary>
    /// <param name="cardDefId"></param>
    private void RemoveFromDeck(CardDefId cardDefId)
    {
        if (!this.deckListByDefId.TryGetValue(cardDefId, out var deckListNodeController))
        {
            // �w�肵��id �����݂��Ȃ�
            return;
        }

        deckListNodeController.RemoveOne();
        if (deckListNodeController.IsEmpty)
        {
            // �f�b�L�ւ̓����������[��
            Destroy(deckListNodeController.gameObject);
            this.deckListByDefId.Remove(cardDefId);
        }

        if (this.cardPoolListByDefId.TryGetValue(cardDefId, out var cardPoolListNodeController))
        {
            cardPoolListNodeController.RemoveOne();
        }

        this.currentDeckTotalCount--;
        this.UpdateDeckTotalCountText();
    }

    /// <summary>
    /// �f�b�L�����̃e�L�X�g���X�V����
    /// </summary>
    private void UpdateDeckTotalCountText()
    {
        this.deckCountText.text = $"{this.ruleBook.MinNumDeckCards} <= {this.currentDeckTotalCount} <= {this.ruleBook.MaxNumDeckCards}";

        this.deckCountText.color = this.IsValidNumCards()
            ? Color.white
            : Color.red;
    }
}
