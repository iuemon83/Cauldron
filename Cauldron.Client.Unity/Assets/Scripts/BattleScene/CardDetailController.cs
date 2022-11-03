using Assets.Scripts;
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
    private Button detailButton = default;
    [SerializeField]
    private GameObject cardTypeSpace = default;
    [SerializeField]
    private GameObject costSpace = default;
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
            this.cardNameText.text = "";
            this.effectText.text = "";
            this.detailButton.gameObject.SetActive(false);
            this.cardTypeSpace.SetActive(false);
            this.costSpace.SetActive(false);
            this.powerSpace.SetActive(false);
            this.toughnessSpace.SetActive(false);

            return;
        }

        this.cardNameText.text = this.source.Name;
        this.effectText.text = Utility.EffectDescription(this.source);
        this.cardTypeText.text = Utility.CardTypeIconUnicode(this.source.Type);
        this.costText.text = this.source.Cost.ToString();

        this.detailButton.gameObject.SetActive(true);
        this.cardTypeSpace.SetActive(true);
        this.costSpace.SetActive(true);

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
