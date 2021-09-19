using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailController : MonoBehaviour
{
    [SerializeField]
    public TextMeshProUGUI cardNameText = default;
    [SerializeField]
    public TextMeshProUGUI effectText = default;

    [SerializeField]
    public Image powerSpace = default;
    [SerializeField]
    public Image toughnessSpace = default;

    [SerializeField]
    public TextMeshProUGUI costText = default;
    [SerializeField]
    public TextMeshProUGUI powerText = default;
    [SerializeField]
    public TextMeshProUGUI toughnessText = default;

    protected Card card;
    protected CardDef cardDef;

    // Update is called once per frame
    void Update()
    {
        if (this.card != null)
        {
            this.cardNameText.text = this.card.Name;
            this.effectText.text = this.card.EffectText;
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
            this.effectText.text = this.cardDef.EffectText;
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
}
