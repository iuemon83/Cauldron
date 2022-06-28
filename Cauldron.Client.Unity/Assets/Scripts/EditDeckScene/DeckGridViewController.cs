using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckGridViewController : MonoBehaviour
{
    [SerializeField]
    private GameObject deckList = default;
    [SerializeField]
    private DeckGridView_ListNodeController listNodePrefab = default;

    private Transform cardPoolListContent;

    private readonly Dictionary<CardDefId, DeckGridView_ListNodeController> deckListByDefId
        = new Dictionary<CardDefId, DeckGridView_ListNodeController>();

    private Action<CardDef> displayCardDetail;
    private Func<CardDefId, int> removeFromDeck;

    // Start is called before the first frame update
    void Start()
    {
        this.cardPoolListContent = this.deckList.transform.Find("Viewport").transform.Find("Content");
    }

    public void Init(
        Dictionary<CardDefId, int> limitNumListById,
        Action<CardDef> displayCardDetail,
        Func<CardDefId, int> removeFromDeck
        )
    {
        this.displayCardDetail = displayCardDetail;
        this.removeFromDeck = removeFromDeck;
    }

    private string SortRank(CardDef cardDef) => cardDef.Cost + cardDef.Name;

    public void RefreshList(IReadOnlyList<CardDef> cards)
    {
        this.RemoveAll();

        foreach (var c in cards.OrderBy(this.SortRank))
        {
            this.CountUp(c);
        }
    }

    private void RemoveAll()
    {
        foreach (var c in this.deckListByDefId.Values)
        {
            Destroy(c.gameObject);
        }

        this.deckListByDefId.Clear();
    }

    /// <summary>
    /// デッキの一覧をソートする
    /// </summary>
    private void SortDeckListNodes()
    {
        var sorted = this.deckListByDefId.Values
            .OrderBy(c => c.SortRank);

        var index = 0;
        foreach (var transform in sorted.Select(c => c.gameObject.transform))
        {
            transform.SetSiblingIndex(index);
            index++;
        }
    }

    public void CountUp(CardDef cardDef)
    {
        // デッキ枚数の上限はチェックしない。デッキ上限枚数の変更で上限を超えることがあるため
        // デッキ投入枚数の上限はチェックしない。同上

        if (!this.deckListByDefId.TryGetValue(cardDef.Id, out var controller))
        {
            controller = this.NewNode(cardDef);
            this.deckListByDefId.Add(cardDef.Id, controller);

            this.SortDeckListNodes();
        }

        controller.CountUp();
    }

    public int CountDown(CardDefId cardDefId)
    {
        if (!this.deckListByDefId.TryGetValue(cardDefId, out var controller))
        {
            return 0;
        }

        var i = controller.CountDown();

        if (controller.IsEmpty)
        {
            this.deckListByDefId.Remove(cardDefId);
            Destroy(controller.gameObject);
        }

        return i;
    }

    private DeckGridView_ListNodeController NewNode(CardDef cardDef)
    {
        var node = Instantiate(this.listNodePrefab, this.cardPoolListContent.transform);
        var controller = node.GetComponent<DeckGridView_ListNodeController>();
        controller.Init(
            cardDef,
            this.displayCardDetail,
            this.removeFromDeck
            );

        return controller;
    }
}
