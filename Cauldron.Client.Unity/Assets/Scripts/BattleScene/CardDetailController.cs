using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI cardNameText = default;
    [SerializeField]
    private TextMeshProUGUI effectText = default;

    [SerializeField]
    private Image powerSpace = default;
    [SerializeField]
    private Image toughnessSpace = default;

    [SerializeField]
    private TextMeshProUGUI costText = default;
    [SerializeField]
    private TextMeshProUGUI powerText = default;
    [SerializeField]
    private TextMeshProUGUI toughnessText = default;

    protected Card card;
    protected CardDef cardDef;

    private Action<Card> displayBigCardDetail;
    private Action<CardDef> displayBigCardDefDetail;

    public void Init(Action<Card> displayBigCardDetail, Action<CardDef> displayBigCardDefDetail)
    {
        this.displayBigCardDefDetail = displayBigCardDefDetail;
        this.displayBigCardDetail = displayBigCardDetail;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.card != null)
        {
            this.cardNameText.text = this.card.Name;
            this.effectText.text = this.card.EffectDescription;
            this.costText.text = this.card.Cost.ToString();

            switch (this.card.Type)
            {
                case CardType.Creature:
                    this.powerText.text = this.card.Power.ToString();
                    this.toughnessText.text = this.card.Toughness.ToString();
                    this.powerSpace.gameObject.SetActive(true);
                    this.toughnessSpace.gameObject.SetActive(true);
                    break;

                default:
                    this.powerSpace.gameObject.SetActive(false);
                    this.toughnessSpace.gameObject.SetActive(false);
                    break;
            }
        }
        else if (this.cardDef != null)
        {
            this.cardNameText.text = this.cardDef.Name;
            this.effectText.text = this.cardDef.EffectDescription;
            this.costText.text = this.cardDef.Cost.ToString();

            switch (this.cardDef.Type)
            {
                case CardType.Creature:
                    this.powerText.text = this.cardDef.Power.ToString();
                    this.toughnessText.text = this.cardDef.Toughness.ToString();
                    this.powerSpace.gameObject.SetActive(true);
                    this.toughnessSpace.gameObject.SetActive(true);
                    break;

                default:
                    this.powerSpace.gameObject.SetActive(false);
                    this.toughnessSpace.gameObject.SetActive(false);
                    break;
            }
        }
    }

    public void SetCard(Card card)
    {
        this.cardDef = null;
        this.card = card;
    }

    public void SetCardDef(CardDef cardDef)
    {
        this.card = null;
        this.cardDef = cardDef;
    }

    public void OnInfoButtonClick()
    {
        if (this.cardDef != null)
        {
            this.displayBigCardDefDetail(this.cardDef);
        }
        else if (this.card != null)
        {
            this.displayBigCardDetail(this.card);
        }
    }
}
