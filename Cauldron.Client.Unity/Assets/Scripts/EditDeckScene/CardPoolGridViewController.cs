using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardPoolGridViewController : MonoBehaviour
{
    [SerializeField]
    private GameObject cardPoolList = default;
    [SerializeField]
    private CardPoolGridView_ListNodeController listNodePrefab = default;

    private Transform cardPoolListContent;

    private readonly Dictionary<CardDefId, CardPoolGridView_ListNodeController> cardPoolListByDefId
        = new Dictionary<CardDefId, CardPoolGridView_ListNodeController>();

    private Action<CardDef> displayCardDetail;
    private Func<CardDef, int> addDeck;
    private string SortRank(CardDef cardDef) => cardDef.Cost.ToString("000") + cardDef.Name;

    // Start is called before the first frame update
    void Start()
    {
        this.cardPoolListContent = this.cardPoolList.transform.Find("Viewport").transform.Find("Content");
    }

    public void Init(
        Action<CardDef> displayCardDetail,
        Func<CardDef, int> addDeck
        )
    {
        this.displayCardDetail = displayCardDetail;
        this.addDeck = addDeck;
    }

    public void RefreshList(IReadOnlyList<(CardDef Card, int CurretNum)> cards)
    {
        this.RemoveAll();

        foreach (var x in cards.OrderBy(x => this.SortRank(x.Card)))
        {
            this.AddToCardPool(x.Card, x.CurretNum);
        }
    }

    private void RemoveAll()
    {
        foreach (var c in this.cardPoolListByDefId.Values)
        {
            Destroy(c.gameObject);
        }

        this.cardPoolListByDefId.Clear();
    }

    /// <summary>
    /// カードプールの一覧にカードを追加する
    /// </summary>
    /// <param name="cardDef"></param>
    private void AddToCardPool(CardDef cardDef, int currentNum)
    {
        var node = Instantiate(this.listNodePrefab, this.cardPoolListContent.transform);
        var controller = node.GetComponent<CardPoolGridView_ListNodeController>();
        controller.Init(
            cardDef,
            currentNum,
            this.displayCardDetail,
            this.addDeck
            );

        this.cardPoolListByDefId.Add(cardDef.Id, controller);
    }

    public void DeckCountUp(CardDefId cardDefId)
    {
        if (this.cardPoolListByDefId.TryGetValue(cardDefId, out var controller))
        {
            controller.DeckCountUp();
        }
    }

    public void DeckCountDown(CardDefId cardDefId)
    {
        if (this.cardPoolListByDefId.TryGetValue(cardDefId, out var controller))
        {
            controller.DeckCountDown();
        }
    }
}
