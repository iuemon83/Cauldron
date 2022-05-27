using Assets.Scripts;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EditDeckSceneController : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPoolList = default;
    [SerializeField]
    private GameObject deckList = default;
    [SerializeField]
    private GameObject listNodePrefab = default;
    [SerializeField]
    private TMP_InputField deckNameInputField = default;
    [SerializeField]
    private TextMeshProUGUI deckCountText = default;
    [SerializeField]
    private CardDefDetailController cardDefDetailController = default;
    [SerializeField]
    private TMP_InputField searchKeywordInputField = default;
    [SerializeField]
    private AudioSource audioSource = default;
    [SerializeField]
    private FlaverTextViewerController flaverTextViewerController = default;

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

        this.searchKeywordInputField.onEndEdit.AddListener(this.SearchKeyWordInputFieldIfPushEnter);

        var holder = ConnectionHolder.Find();
        this.ruleBook = await holder.Client.GetRuleBook();
        this.allCards = holder.CardPool.Values
            .OrderBy(c => c.Cost)
            .ThenBy(c => c.FullName)
            .ToArray();

        this.numCardsLimitListByCardDefId = this.allCards.ToDictionary(c => c.Id, c => c.LimitNumCardsInDeck.Value);

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

    private void SearchKeyWordInputFieldIfPushEnter(string a)
    {
        if (Input.GetKey(KeyCode.Return))
        {
            this.OnSearchButtonClick();
        }
    }

    public void OnSearchButtonClick()
    {
        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

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
            || cardDef.Annotations.Any(a => a.Contains(keyword))
            || cardDef.Abilities.Any(a => Utility.DisplayText(a).Contains(keyword))
            || cardDef.FlavorText.Contains(keyword)
            ;
    }

    /// <summary>
    /// 作成済みのデッキを画面に反映する
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
        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

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

        await Utility.LoadAsyncScene(SceneNames.ListDeckScene);
    }

    public async void OnCancelButtonClick()
    {
        this.PlayAudio(SeAudioCache.SeAudioType.Ok);

        await Utility.LoadAsyncScene(SceneNames.ListDeckScene);
    }

    /// <summary>
    /// カードプールの一覧にカードを追加する
    /// </summary>
    /// <param name="cardDef"></param>
    private void AddToCardPool(CardDef cardDef)
    {
        if (!this.numCardsLimitListByCardDefId.TryGetValue(cardDef.Id, out var limit))
        {
            Debug.Log($"指定されたIDが存在しない。CardDefId={cardDef.Id}");
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
            this.cardDefDetailController.SetCard,
            this.flaverTextViewerController.Open
            );

        this.cardPoolListByDefId.Add(cardDef.Id, controller);
    }

    /// <summary>
    /// デッキ画面側にカードを追加する
    /// </summary>
    /// <param name="cardDefId"></param>
    private void AddToDeck(CardDef cardDef)
    {
        if (this.IsLimitTotalNum)
        {
            Debug.Log("デッキ上限");
            return;
        }

        if (!this.numCardsLimitListByCardDefId.TryGetValue(cardDef.Id, out var limit))
        {
            Debug.Log($"指定されたIDが存在しない。CardDefId={cardDef.Id}");
            return;
        }

        if (limit == 0)
        {
            Debug.Log("デッキに投入できる枚数が0枚");
            return;
        }

        if (!this.deckListByDefId.TryGetValue(cardDef.Id, out var deckListNodeController))
        {
            // 指定したid が存在しない
            //まだデッキに投入されていない
            var node = Instantiate(this.listNodePrefab, this.deckListContent.transform);
            deckListNodeController = node.GetComponent<EditDeckScene_ListNodeController>();
            deckListNodeController.Init(
                cardDef,
                limit,
                0,
                () => this.AddToDeck(cardDef),
                () => this.RemoveFromDeck(cardDef.Id),
                this.cardDefDetailController.SetCard,
                this.flaverTextViewerController.Open
                );

            this.deckListByDefId.Add(cardDef.Id, deckListNodeController);

            this.SortDeckListNodes();
        }

        if (deckListNodeController.IsLimitNumByCard)
        {
            Debug.Log("デッキに投入できる上限枚数");
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
    /// デッキの一覧をソートする
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
    /// デッキ画面側からカードを削除する
    /// </summary>
    /// <param name="cardDefId"></param>
    private void RemoveFromDeck(CardDefId cardDefId)
    {
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

        if (this.cardPoolListByDefId.TryGetValue(cardDefId, out var cardPoolListNodeController))
        {
            cardPoolListNodeController.RemoveOne();
        }

        this.currentDeckTotalCount--;
        this.UpdateDeckTotalCountText();
    }

    /// <summary>
    /// デッキ枚数のテキストを更新する
    /// </summary>
    private void UpdateDeckTotalCountText()
    {
        this.deckCountText.text = $"{this.ruleBook.MinNumDeckCards} <= {this.currentDeckTotalCount} <= {this.ruleBook.MaxNumDeckCards}";

        this.deckCountText.color = this.IsValidNumCards()
            ? Color.white
            : Color.red;
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
