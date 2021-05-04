using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using UnityEngine;
using UnityEngine.UI;

public class CardDefDetailController : MonoBehaviour
{
    public Text CardName;
    public Text FlavorText;

    public GameObject PowerSpace;
    public GameObject ToughnessSpace;

    public Text Cost;
    public Text Power;
    public Text Toughness;

    protected CardDef source;

    // Update is called once per frame
    void Update()
    {
        if (this.source == null)
        {
            return;
        }

        this.CardName.text = this.source.Name;
        this.FlavorText.text = this.source.FlavorText;
        this.Cost.text = this.source.Cost.ToString();

        switch (this.source.Type)
        {
            case CardType.Creature:
                this.Power.text = this.source.Power.ToString();
                this.Toughness.text = this.source.Toughness.ToString();
                this.PowerSpace.SetActive(true);
                this.ToughnessSpace.SetActive(true);
                break;

            default:
                this.PowerSpace.SetActive(false);
                this.ToughnessSpace.SetActive(false);
                break;
        }
    }

    public void SetCard(CardDef cardDef)
    {
        this.source = cardDef;
    }
}
