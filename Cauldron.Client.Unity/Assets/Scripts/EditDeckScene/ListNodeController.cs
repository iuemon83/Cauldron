using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListNodeController : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField]
    private Text CardNameText;
    [SerializeField]
    private Text CardStatsText;
    [SerializeField]
    private Text CurrentDeckCountText;
    [SerializeField]
    private Text LimitDeckCountText;

    private Action AddToDeckAction { get; set; }
    private Action RemoveFromDeckAction { get; set; }
    private Action<CardDef> ShowDetailAction { get; set; }


    public int CurrentDeckCount => int.TryParse(this.CurrentDeckCountText.text, out var intValue) ? intValue : 0;

    public int LimitDeckCount => int.TryParse(this.LimitDeckCountText.text, out var intValue) ? intValue : 0;

    public bool IsLimitNumByCard => this.CurrentDeckCount == this.LimitDeckCount;

    public bool IsEmpty => this.CurrentDeckCount == 0;

    public CardDef Source { get; set; }

    public void Init(CardDef source, int limit, Action AddToDeckAction, Action RemoveFromDeckAction, Action<CardDef> ShowDetailAction)
    {
        this.Source = source;
        this.AddToDeckAction = AddToDeckAction;
        this.RemoveFromDeckAction = RemoveFromDeckAction;
        this.ShowDetailAction = ShowDetailAction;

        this.CardNameText.text = $"{this.Source.Name}";
        this.CardStatsText.text = this.Source.Type == CardType.Creature
            ? $"{this.Source.Cost}/{this.Source.Power}/{this.Source.Toughness}"
            : $"{this.Source.Cost}/-/-";
        this.CurrentDeckCountText.text = "0";
        this.LimitDeckCountText.text = $"{(this.Source.IsToken ? 0 : limit)}";
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

    public void AddOne()
    {
        this.CurrentDeckCountText.text = Math.Min(this.LimitDeckCount, this.CurrentDeckCount + 1).ToString();
    }

    public void RemoveOne()
    {
        this.CurrentDeckCountText.text = Math.Max(0, this.CurrentDeckCount - 1).ToString();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("enter pointer ! ");
        this.ShowDetailAction(this.Source);
    }
}
