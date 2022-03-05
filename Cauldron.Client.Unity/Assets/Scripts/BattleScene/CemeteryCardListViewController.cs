using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CemeteryCardListViewController : MonoBehaviour
{
    [SerializeField]
    private ScrollRect cardList = default;
    [SerializeField]
    private CemeteryCardListView_ListNodeController listNodePrefab = default;
    [SerializeField]
    private CardDetailController cardDetailController = default;
    [SerializeField]
    private Transform hiddenContainer = default;
    [SerializeField]
    private Transform displayContainer = default;
    [SerializeField]
    private Image backgroundImage = default;
    [SerializeField]
    private Image controllerIconImage = default;

    private Transform actionLogListContent;
    private bool isDisplay = false;

    private readonly Dictionary<CardId, CemeteryCardListView_ListNodeController> dic
        = new Dictionary<CardId, CemeteryCardListView_ListNodeController>();

    public void Start()
    {
        this.actionLogListContent = this.cardList.transform.Find("Viewport").transform.Find("Content");
    }

    public void InitAsYou()
    {
        this.backgroundImage.color = BattleSceneController.Instance.YouColor;
        var (controllerSuccess, controllerIcon) = ControllerIconCache.TryGet(ControllerIconCache.IconType.You);
        if (controllerSuccess)
        {
            this.controllerIconImage.sprite = controllerIcon;
        }
    }

    public void InitAsOpponent()
    {
        this.backgroundImage.color = BattleSceneController.Instance.OpponentColor;
        var (controllerSuccess, controllerIcon) = ControllerIconCache.TryGet(ControllerIconCache.IconType.Opponent);
        if (controllerSuccess)
        {
            this.controllerIconImage.sprite = controllerIcon;
        }
    }

    public void AddCard(Card card)
    {
        this.AddListNode(card);
    }

    public void RemoveCard(Card card)
    {
        this.RemoveListNode(card);
    }

    public void ToggleDisplay()
    {
        if (this.isDisplay)
        {
            this.Hidden();
        }
        else
        {
            this.Display();
        }
    }

    public void Display()
    {
        this.transform.SetParent(this.displayContainer.transform, false);
        this.isDisplay = true;
    }

    public void Hidden()
    {
        this.transform.SetParent(this.hiddenContainer.transform, false);
        this.isDisplay = false;
    }

    private void AddListNode(Card card)
    {
        if (this.actionLogListContent == null)
        {
            return;
        }

        var controller = Instantiate(this.listNodePrefab, this.actionLogListContent.transform);
        controller.Init(card,
            this.cardDetailController.SetCard
            );

        this.dic.Add(card.Id, controller);
    }

    private void RemoveListNode(Card card)
    {
        if (this.dic.TryGetValue(card.Id, out var nodeController))
        {
            Destroy(nodeController.gameObject);
            this.dic.Remove(card.Id);
        }
    }
}
