using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChoiceDialog_ListNodeController : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private TextMeshProUGUI cardNameText;
    [SerializeField]
    private TextMeshProUGUI cardStatsText;
    [SerializeField]
    private TextMeshProUGUI currentCountText;
    [SerializeField]
    private TextMeshProUGUI limitCountText;

    private Action AddToDeckAction { get; set; }
    private Action RemoveFromDeckAction { get; set; }
    private Action<CardDef> ShowDetailAction { get; set; }

    public int CurrentCount => int.TryParse(this.currentCountText.text, out var intValue) ? intValue : 0;

    public int LimitCount => int.TryParse(this.limitCountText.text, out var intValue) ? intValue : 0;

    public bool IsLimitNumByCard => this.CurrentCount == this.LimitCount;

    public bool IsEmpty => this.CurrentCount == 0;

    public CardDef Source { get; set; }

    public void Init(CardDef source, int limit, Action AddToDeckAction, Action RemoveFromDeckAction, Action<CardDef> ShowDetailAction)
    {
        this.Source = source;
        this.AddToDeckAction = AddToDeckAction;
        this.RemoveFromDeckAction = RemoveFromDeckAction;
        this.ShowDetailAction = ShowDetailAction;

        this.cardNameText.text = $"{this.Source.Name}";
        this.cardStatsText.text = this.Source.Type == CardType.Creature
            ? $"{this.Source.Cost}/{this.Source.Power}/{this.Source.Toughness}"
            : $"{this.Source.Cost}/-/-";
        this.currentCountText.text = "0";
        this.limitCountText.text = $"{limit}";
    }

    /// <summary>
    /// カード枚数の入力イベント
    /// </summary>
    public void OnAddButtonClick()
    {
        this.AddToDeckAction();
    }

    /// <summary>
    /// カード枚数の入力イベント
    /// </summary>
    public void OnRemoveButtonClick()
    {
        this.RemoveFromDeckAction();
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
        this.ShowDetailAction(this.Source);
    }
}
