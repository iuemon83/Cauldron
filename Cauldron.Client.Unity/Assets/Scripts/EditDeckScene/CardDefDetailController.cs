using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDefDetailController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI cardNameText;
    [SerializeField]
    private TextMeshProUGUI flavorText;
    [SerializeField]
    private TextMeshProUGUI effectText;

    [SerializeField]
    private Image powerSpace;
    [SerializeField]
    private Image toughnessSpace;

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
        this.effectText.text = this.source.EffectText;
        this.costText.text = this.source.Cost.ToString();

        switch (this.source.Type)
        {
            case CardType.Creature:
                this.powerText.text = this.source.Power.ToString();
                this.toughnessText.text = this.source.Toughness.ToString();
                this.powerSpace.gameObject.SetActive(true);
                this.toughnessSpace.gameObject.SetActive(true);
                break;

            default:
                this.powerSpace.gameObject.SetActive(false);
                this.toughnessSpace.gameObject.SetActive(false);
                break;
        }
    }

    public void SetCard(CardDef cardDef)
    {
        this.source = cardDef;
    }
}
