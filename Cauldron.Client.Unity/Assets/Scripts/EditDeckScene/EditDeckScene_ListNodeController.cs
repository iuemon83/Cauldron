using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditDeckScene_ListNodeController : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private TextMeshProUGUI cardNameText = default;
    [SerializeField]
    private TextMeshProUGUI cardStatsText = default;
    [SerializeField]
    private TextMeshProUGUI currentDeckCountText = default;
    [SerializeField]
    private TextMeshProUGUI limitDeckCountText = default;

    private Action AddToDeckAction { get; set; }
    private Action RemoveFromDeckAction { get; set; }
    private Action<CardDef> ShowDetailAction { get; set; }
    private Action<CardDef> ShowFlaverTextAction { get; set; }

    public int CurrentDeckCount => int.TryParse(this.currentDeckCountText.text, out var intValue) ? intValue : 0;

    public int LimitDeckCount => int.TryParse(this.limitDeckCountText.text, out var intValue) ? intValue : 0;

    public bool IsLimitNumByCard => this.CurrentDeckCount == this.LimitDeckCount;

    public bool IsEmpty => this.CurrentDeckCount == 0;

    public CardDef Source { get; set; }

    public void Init(CardDef source, int limit, int currentNum,
        Action AddToDeckAction, Action RemoveFromDeckAction,
        Action<CardDef> ShowDetailAction,
        Action<CardDef> ShowFlaverTextAction
        )
    {
        this.Source = source;
        this.AddToDeckAction = AddToDeckAction;
        this.RemoveFromDeckAction = RemoveFromDeckAction;
        this.ShowDetailAction = ShowDetailAction;
        this.ShowFlaverTextAction = ShowFlaverTextAction;

        this.cardNameText.text = $"{this.Source.Name}";
        this.cardStatsText.text = this.Source.Type == CardType.Creature
            ? $"{this.Source.Cost}/{this.Source.Power}/{this.Source.Toughness}"
            : $"{this.Source.Cost}/-/-";

        this.limitDeckCountText.text = limit.ToString();
        this.UpdateCurrentDeckCount(currentNum);
    }

    /// <summary>
    /// カード枚数の入力イベント
    /// </summary>
    public void OnAddButtonClick()
    {
        Debug.Log("add button click! ");

        this.AddToDeckAction();
    }

    /// <summary>
    /// カード枚数の入力イベント
    /// </summary>
    public void OnRemoveButtonClick()
    {
        Debug.Log("remove button click! ");

        this.RemoveFromDeckAction();
    }

    public void OnShowFlaverTextButtonClick()
    {
        Debug.Log("show flaver text button click! ");

        this.ShowFlaverTextAction(this.Source);
    }

    public void AddOne()
    {
        this.UpdateCurrentDeckCount(Math.Min(this.LimitDeckCount, this.CurrentDeckCount + 1));
    }

    public void RemoveOne()
    {
        this.UpdateCurrentDeckCount(Math.Max(0, this.CurrentDeckCount - 1));
    }

    private void UpdateCurrentDeckCount(int x)
    {
        this.currentDeckCountText.text = x.ToString();

        this.currentDeckCountText.color = this.CurrentDeckCount >= this.LimitDeckCount
            ? Color.red
            : Color.black;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        this.ShowDetailAction(this.Source);
    }
}
