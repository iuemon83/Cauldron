using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDefDetailController : MonoBehaviour
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

    protected CardDef source;

    // Update is called once per frame
    void Update()
    {
        if (this.source == null)
        {
            return;
        }

        this.cardNameText.text = this.source.Name;
        this.effectText.text = this.source.EffectDescription;
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
