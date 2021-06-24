using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChoiceDialog_ListNodeController : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private TextMeshProUGUI cardNameText;
    [SerializeField]
    private TextMeshProUGUI currentCountText;
    [SerializeField]
    private TextMeshProUGUI limitCountText;
    [SerializeField]
    private Image controllerIconImage;
    [SerializeField]
    private Image zoneIconImage;

    private Action addToDeckAction;
    private Action removeFromDeckAction;
    private Action<Card> showCardDetailAction;
    private Action<CardDef> showCardDefDetailAction;

    public int CurrentCount => int.TryParse(this.currentCountText.text, out var intValue) ? intValue : 0;

    public int LimitCount => int.TryParse(this.limitCountText.text, out var intValue) ? intValue : 0;

    public bool IsLimitNumByCard => this.CurrentCount == this.LimitCount;

    public bool IsEmpty => this.CurrentCount == 0;

    public CardDef SourceCardDef { get; private set; }

    public Card SourceCard { get; private set; }

    public void Init(CardDef source, int limit, Action AddToDeckAction, Action RemoveFromDeckAction, Action<CardDef> ShowDetailAction)
    {
        this.SourceCardDef = source;
        this.addToDeckAction = AddToDeckAction;
        this.removeFromDeckAction = RemoveFromDeckAction;
        this.showCardDefDetailAction = ShowDetailAction;

        this.cardNameText.text = $"{this.SourceCardDef.Name}";
        this.currentCountText.text = "0";
        this.limitCountText.text = $"{limit}";

        var (controllerSuccess, controllerIcon) = ControllerIconCache.TryGet(ControllerIconCache.IconType.You);
        if (controllerSuccess)
        {
            this.controllerIconImage.sprite = controllerIcon;
        }

        var (zoneSuccess, zoneIcon) = ZoneIconCache.TryGet(ZoneName.CardPool);
        if (zoneSuccess)
        {
            this.controllerIconImage.sprite = zoneIcon;
        }
    }

    public void Init(Card source, int limit, Action AddToDeckAction, Action RemoveFromDeckAction, Action<Card> ShowDetailAction)
    {
        this.SourceCard = source;
        this.addToDeckAction = AddToDeckAction;
        this.removeFromDeckAction = RemoveFromDeckAction;
        this.showCardDetailAction = ShowDetailAction;

        this.cardNameText.text = $"{this.SourceCard.Name}";
        this.currentCountText.text = "0";
        this.limitCountText.text = $"{limit}";

        var controllerIconType = BattleSceneController.Instance.YouId == source.OwnerId
            ? ControllerIconCache.IconType.You
            : ControllerIconCache.IconType.Opponent;

        var (controllerSuccess, controllerIcon) = ControllerIconCache.TryGet(controllerIconType);
        if (controllerSuccess)
        {
            this.controllerIconImage.sprite = controllerIcon;
        }

        var (zoneSuccess, zoneIcon) = ZoneIconCache.TryGet(source.Zone.ZoneName);
        if (zoneSuccess)
        {
            this.zoneIconImage.sprite = zoneIcon;
        }
    }

    /// <summary>
    /// カード枚数の入力イベント
    /// </summary>
    public void OnAddButtonClick()
    {
        this.addToDeckAction();
    }

    /// <summary>
    /// カード枚数の入力イベント
    /// </summary>
    public void OnRemoveButtonClick()
    {
        this.removeFromDeckAction();
    }

    public void AddOne()
    {
        this.UpdateCountText(Math.Min(this.LimitCount, this.CurrentCount + 1));
    }

    public void RemoveOne()
    {
        this.UpdateCountText(Math.Max(0, this.CurrentCount - 1));
    }

    private void UpdateCountText(int count)
    {
        this.currentCountText.text = count.ToString();
        this.currentCountText.color = this.IsLimitNumByCard
            ? Color.red
            : Color.black;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (this.SourceCardDef != default)
        {
            this.showCardDefDetailAction(this.SourceCardDef);
        }
        else if (this.SourceCard != default)
        {
            this.showCardDetailAction(this.SourceCard);
        }
    }
}
