using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using TMPro;
using UnityEngine;

public class CardDetailController : MonoBehaviour
{
    public TextMeshProUGUI CardNameText;
    public TextMeshProUGUI FlavorText;

    public GameObject PowerSpace;
    public GameObject ToughnessSpace;

    public TextMeshProUGUI CostText;
    public TextMeshProUGUI PowerText;
    public TextMeshProUGUI ToughnessText;

    protected Card card;
    protected CardDef cardDef;

    // Update is called once per frame
    void Update()
    {
        if (this.card != null)
        {
            this.CardNameText.text = this.card.Name;
            this.FlavorText.text = this.card.FlavorText;
            this.CostText.text = this.card.Cost.ToString();

            switch (this.card.Type)
            {
                case CardType.Creature:
                    this.PowerText.text = this.card.Power.ToString();
                    this.ToughnessText.text = this.card.Toughness.ToString();
                    this.PowerSpace.SetActive(true);
                    this.ToughnessSpace.SetActive(true);
                    break;

                default:
                    this.PowerSpace.SetActive(false);
                    this.ToughnessSpace.SetActive(false);
                    break;
            }
        }
        else if (this.cardDef != null)
        {
            this.CardNameText.text = this.cardDef.Name;
            this.FlavorText.text = this.cardDef.FlavorText;
            this.CostText.text = this.cardDef.Cost.ToString();

            switch (this.card.Type)
            {
                case CardType.Creature:
                    this.PowerText.text = this.cardDef.Power.ToString();
                    this.ToughnessText.text = this.cardDef.Toughness.ToString();
                    this.PowerSpace.SetActive(true);
                    this.ToughnessSpace.SetActive(true);
                    break;

                default:
                    this.PowerSpace.SetActive(false);
                    this.ToughnessSpace.SetActive(false);
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
