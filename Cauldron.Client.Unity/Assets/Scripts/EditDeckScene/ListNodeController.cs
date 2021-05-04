using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListNodeController : MonoBehaviour, IPointerEnterHandler
{
    public Text CardNameText;
    public Text CardStatsText;
    public Text CurrentDeckCountText;
    public Text LimitDeckCountText;

    public Action AddToDeckAction { get; set; }
    public Action RemoveFromDeckAction { get; set; }
    public Action<CardDef> ShowDetailAction { get; set; }


    public int CurrentDeckCount => int.TryParse(this.CurrentDeckCountText.text, out var intValue) ? intValue : 0;

    public int LimitDeckCount => int.TryParse(this.LimitDeckCountText.text, out var intValue) ? intValue : 0;

    public bool IsLimit => this.CurrentDeckCount == this.LimitDeckCount;

    public bool IsEmpty => this.CurrentDeckCount == 0;

    public CardDef Source { get; set; }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Set(CardDef source, int limit)
    {
        this.Source = source;

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
