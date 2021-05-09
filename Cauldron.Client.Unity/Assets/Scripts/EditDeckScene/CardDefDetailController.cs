using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using TMPro;
using UnityEngine;

public class CardDefDetailController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI cardNameText;
    [SerializeField]
    private TextMeshProUGUI flavorText;

    [SerializeField]
    private GameObject powerSpace;
    [SerializeField]
    private GameObject toughnessSpace;

    [SerializeField]
    private TextMeshProUGUI costText;
    [SerializeField]
    private TextMeshProUGUI powerText;
    [SerializeField]
    private TextMeshProUGUI toughnessText;

    protected CardDef source;

    // Update is called once per frame
    void Update()
    {
        if (this.source == null)
        {
            return;
        }

        this.cardNameText.text = this.source.Name;
        this.flavorText.text = this.source.FlavorText;
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

    public void SetCard(CardDef cardDef)
    {
        this.source = cardDef;
    }
}
