using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using UnityEngine;
using UnityEngine.UI;

public class CardDetailController : MonoBehaviour
{
    public Text CardName;
    public Text FlavorText;

    public GameObject PowerSpace;
    public GameObject ToughnessSpace;

    public Text Cost;
    public Text Power;
    public Text Toughness;

    protected Card card;

    // Update is called once per frame
    void Update()
    {
        if (this.card == null)
        {
            return;
        }

        this.CardName.text = this.card.Name;
        this.FlavorText.text = this.card.FlavorText;
        this.Cost.text = this.card.Cost.ToString();

        switch (this.card.Type)
        {
            case CardType.Creature:
                this.Power.text = this.card.Power.ToString();
                this.Toughness.text = this.card.Toughness.ToString();
                this.PowerSpace.SetActive(true);
                this.ToughnessSpace.SetActive(true);
                break;

            default:
                this.PowerSpace.SetActive(false);
                this.ToughnessSpace.SetActive(false);
                break;
        }
    }

    public void SetCard(Card card)
    {
        this.card = card;
    }
}
