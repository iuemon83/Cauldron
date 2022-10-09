using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;

public class CardDetailController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI cardNameText = default;
    [SerializeField]
    private TextMeshProUGUI effectText = default;

    [SerializeField]
    private GameObject powerSpace = default;
    [SerializeField]
    private GameObject toughnessSpace = default;

    [SerializeField]
    private TextMeshProUGUI cardTypeText = default;
    [SerializeField]
    private TextMeshProUGUI costText = default;
    [SerializeField]
    private TextMeshProUGUI powerText = default;
    [SerializeField]
    private TextMeshProUGUI toughnessText = default;

    protected CardBridge source;

    private Action<CardBridge> displayBigCardDetail;

    public void Init(Action<CardBridge> displayBigCardDetail)
    {
        this.displayBigCardDetail = displayBigCardDetail;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.source == null)
        {
            return;
        }

        this.cardNameText.text = this.source.Name;
        this.effectText.text = Utility.EffectDescription(this.source);
        this.cardTypeText.text = Utility.CardTypeIconUnicode(this.source.Type);
        this.costText.text = this.source.Cost.ToString();

        switch (this.source.Type)
        {
            case CardType.Creature:
                this.powerText.text = this.source.Power.ToString();
                this.toughnessText.text = this.source.Toughness.ToString();
                this.powerSpace.SetActive(true);
                this.toughnessSpace.SetActive(true);
                break;

            default:
                this.powerSpace.SetActive(false);
                this.toughnessSpace.SetActive(false);
                break;
        }
    }

    public void SetCard(Card card)
    {
        this.source = new CardBridge(default, card);
    }

    public void SetCardDef(CardDef cardDef)
    {
        this.source = new CardBridge(cardDef, default);
    }

    public void OnInfoButtonClick()
    {
        this.displayBigCardDetail(this.source);
    }
}
