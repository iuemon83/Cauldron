using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditDeckSceneController : MonoBehaviour
{
    public static Color LimitNumTextColorToken = Color.magenta;
    public static Color PickNumTextColorLimit = Color.green;
    public static Color PickNumTextColorOver = Color.magenta;
    public static Color PickNumTextColorNormal = Color.white;

    [SerializeField]
    private TMP_InputField deckNameInputField = default;
    [SerializeField]
    private TextMeshProUGUI deckCountText = default;
    [SerializeField]
    private CardDefDetailController cardDefDetailController = default;
    [SerializeField]
    private TMP_InputField searchKeywordInputField = default;
    [SerializeField]
    private FlaverTextViewerController flaverTextViewerController = default;
    [SerializeField]
    private CardPoolGridViewController cardPoolGridViewController = default;
    [SerializeField]
    private DeckGridViewController deckGridViewController = default;
    [SerializeField]
    private Toggle isShowTokenToggle = default;

    public IDeck DeckToEdit { get; set; }

    private int currentDeckTotalCount;

    private bool IsLimitTotalNum => this.currentDeckTotalCount == this.ruleBook.MaxNumDeckCards;

    private RuleBook ruleBook;

    private IReadOnlyDictionary<CardDefId, CardDef> allCardsById
        = new Dictionary<CardDefId, CardDef>();

    private readonly List<CardDefId> deckCards = new List<CardDefId>();

    // Start is called before the first frame update
    async void Start()
    {
        this.searchKeywordInputField.onEndEdit.AddListener(this.SearchKeyWordInputFieldIfPushEnter);

        var holder = ConnectionHolder.Find();
        this.ruleBook = await holder.Client.GetRuleBook();
        this.allCardsById = holder.CardPool;

        this.flaverTextViewerController.Init(
            this.PrevCard,
            this.NextCard
            );

        this.cardPoolGridViewController.Init(
            this.DisplayCardDetail,
            this.AddToDeck
            );

        this.deckGridViewController.Init(
            this.allCardsById.ToDictionary(x => x.Key, x => x.Value.LimitNumCardsInDeck.Value),
            this.DisplayCardDetail,
            this.RemoveFromDeck
            );

        this.cardDefDetailController.Init(
            defId => this.AddToDeck(this.allCardsById[defId]),
            defId => this.RemoveFromDeck(defId),
            this.DisplayFlaverTextViewer
            );

        var cardPoolNodes = this.allCardsById.Values
            .Select(c => (c, 0))
            .ToArray();

        this.cardPoolGridViewController.RefreshList(cardPoolNodes);

        if (this.DeckToEdit != null)
        {
            this.ApplyDeck(this.DeckToEdit);
        }

        this.UpdateDeckTotalCountText();
    }

    private void DisplayFlaverTextViewer(CardDefId cardDefId, int cardPoolIndex)
    {
        this.flaverTextViewerController.Open(this.allCardsById[cardDefId], cardPoolIndex);
    }

    private void SearchKeyWordInputFieldIfPushEnter(string a)
    {
        if (Input.GetKey(KeyCode.Return))
        {
            this.OnSearchButtonClick();
        }
    }

    public void OnIsShowTokenToggleChanged()
    {
        this.RefleshCardPool();
    }

    public void OnSearchButtonClick()
    {
        AudioController.CreateOrFind().PlayAudio(SeAudioCache.SeAudioType.Ok);

        this.RefleshCardPool();
    }

    private void RefleshCardPool()
    {
        var keyword = this.searchKeywordInputField.text;
        var isShowToken = this.isShowTokenToggle.isOn;

        var matchedCards = this.allCardsById.Values;

        if (!isShowToken)
        {
            matchedCards = matchedCards
                .Where(c => !c.IsToken);
        }

        var matchedCardsArray = matchedCards
            .Where(c => this.IsMatchedByKeyword(keyword, c))
            .Select(c => (c, this.deckCards.Count(id => id == c.Id)))
            .ToArray();

        this.cardPoolGridViewController.RefreshList(matchedCardsArray);
    }

    private bool IsMatchedByKeyword(string keyword, CardDef cardDef)
    {
        var normalizedList = this.NormalizeKeyword(keyword);

        return normalizedList
            .All(w =>
            {
                return this.NormalizeForKeyword(cardDef.Name).Contains(w)
                    || this.NormalizeForKeyword(Utility.EffectDescription(new CardBridge(cardDef, default))).Contains(w)
                    || cardDef.Annotations.Any(a => this.NormalizeForKeyword(a).Contains(w))
                    || cardDef.Abilities.Any(a => this.NormalizeForKeyword(Utility.DisplayText(a)).Contains(w))
                    ;
            });
    }

    private string[] NormalizeKeyword(string value)
    {
        return value.Split(' ', '　')
            .Select(this.NormalizeForKeyword)
            .ToArray();
    }

    private string NormalizeForKeyword(string value)
    {
        return value
            .ToLower()
            .Replace(" ", "")
            .Replace("　", "")
            .Replace(".", "");
    }

    /// <summary>
    /// 作成済みのデッキを画面に反映する
    /// </summary>
    /// <param name="deck"></param>
    /// <param name="cardPool"></param>
    private void ApplyDeck(IDeck deck)
    {
        this.deckNameInputField.text = deck.Name;

        foreach (var cardName in deck.CardDefNames)
        {
            var cardDef = this.allCardsById.Values.FirstOrDefault(cd => cd.FullName == cardName);
            if (cardDef != null)
            {
                this.AddToDeckIgnoreLimit(cardDef);
            }
        }
    }

    private bool IsValidNumCards() => this.currentDeckTotalCount >= this.ruleBook.MinNumDeckCards
        && this.currentDeckTotalCount <= this.ruleBook.MaxNumDeckCards;

    public async void OnSaveButtonClick()
    {
        AudioController.CreateOrFind().PlayAudio(SeAudioCache.SeAudioType.Ok);

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

        var deckCards = this.deckCards.Select(id => this.allCardsById[id]).ToArray();

        if (this.DeckToEdit == null)
        {
            new DeckRepository().Add(deckName, deckCards);
        }
        else
        {
            new DeckRepository().Update(this.DeckToEdit.Id, deckName, deckCards);
        }

        await Utility.LoadAsyncScene(SceneNames.ListDeckScene);
    }

    public async void OnCancelButtonClick()
    {
        AudioController.CreateOrFind().PlayAudio(SeAudioCache.SeAudioType.Ok);

        await Utility.LoadAsyncScene(SceneNames.ListDeckScene);
    }

    private void DisplayCardDetail(CardDef cardDef)
    {
        var cardPoolIndex = this.cardPoolGridViewController.IndexOf(cardDef);

        this.cardDefDetailController.SetCard(cardDef, cardPoolIndex);
    }

    /// <summary>
    /// デッキ画面側にカードを追加する
    /// </summary>
    /// <param name="cardDefId"></param>
    private int AddToDeck(CardDef cardDef)
    {
        if (this.IsLimitTotalNum)
        {
            Debug.Log("デッキ上限");
            return 0;
        }

        if (!this.allCardsById.TryGetValue(cardDef.Id, out var card))
        {
            Debug.Log($"指定されたIDが存在しない。CardDefId={cardDef.Id}");
            return 0;
        }

        if (card.LimitNumCardsInDeck.Value == 0)
        {
            Debug.Log("デッキに投入できる枚数が0枚");
            return 0;
        }

        var current = this.deckCards.Count(x => x == cardDef.Id);
        if (current >= card.LimitNumCardsInDeck.Value)
        {
            Debug.Log("デッキに投入できる枚数の上限");
            return 0;
        }

        return this.AddToDeckIgnoreLimit(cardDef);
    }

    private int AddToDeckIgnoreLimit(CardDef cardDef)
    {
        this.deckGridViewController.CountUp(cardDef);

        if (this.cardPoolGridViewController.isActiveAndEnabled)
        {
            this.cardPoolGridViewController.DeckCountUp(cardDef.Id);
        }

        this.currentDeckTotalCount++;
        this.UpdateDeckTotalCountText();

        this.deckCards.Add(cardDef.Id);

        return 1;
    }

    /// <summary>
    /// デッキ画面側からカードを削除する
    /// </summary>
    /// <param name="cardDefId"></param>
    private int RemoveFromDeck(CardDefId cardDefId)
    {
        var actualRemovedNum = this.deckGridViewController.CountDown(cardDefId);
        if (actualRemovedNum == 0)
        {
            return 0;
        }

        this.deckCards.Remove(cardDefId);

        this.currentDeckTotalCount--;
        this.UpdateDeckTotalCountText();

        this.cardPoolGridViewController.DeckCountDown(cardDefId);

        return 1;
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

    private (CardDef, int) PrevCard(int index)
    {
        return this.cardPoolGridViewController.PrevCard(index);
    }

    private (CardDef, int) NextCard(int index)
    {
        return this.cardPoolGridViewController.NextCard(index);
    }
}
