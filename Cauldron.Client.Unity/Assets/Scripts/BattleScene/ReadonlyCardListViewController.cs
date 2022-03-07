using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadonlyCardListViewController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI titleText = default;
    [SerializeField]
    private TextMeshProUGUI numCardsText = default;
    [SerializeField]
    private ScrollRect cardList = default;
    [SerializeField]
    private ReadonlyCardListView_ListNodeController listNodePrefab = default;
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

    private readonly Dictionary<CardId, ReadonlyCardListView_ListNodeController> dic
        = new Dictionary<CardId, ReadonlyCardListView_ListNodeController>();

    public void Start()
    {
        this.actionLogListContent = this.cardList.transform.Find("Viewport").transform.Find("Content");
    }

    public void InitAsYou(string title)
    {
        this.titleText.text = title ?? "";
        this.numCardsText.text = "[0]";
        this.backgroundImage.color = BattleSceneController.Instance.YouColor;
        var (controllerSuccess, controllerIcon) = ControllerIconCache.TryGet(ControllerIconCache.IconType.You);
        if (controllerSuccess)
        {
            this.controllerIconImage.sprite = controllerIcon;
        }
    }

    public void InitAsOpponent(string title)
    {
        this.titleText.text = title ?? "";
        this.numCardsText.text = "[0]";
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
        this.numCardsText.text = $"[{this.dic.Count}]";
    }

    private void RemoveListNode(Card card)
    {
        if (this.dic.TryGetValue(card.Id, out var nodeController))
        {
            Destroy(nodeController.gameObject);
            this.dic.Remove(card.Id);
        }
        this.numCardsText.text = $"[{this.dic.Count}]";
    }
}
