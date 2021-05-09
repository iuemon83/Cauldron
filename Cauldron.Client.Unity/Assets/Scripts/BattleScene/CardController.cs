using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    public TextMeshProUGUI CardNameText;

    public GameObject PowerSpace;
    public GameObject ToughnessSpace;

    public TextMeshProUGUI CostText;
    public TextMeshProUGUI PowerText;
    public TextMeshProUGUI ToughnessText;

    public Image CardImage;

    public GameObject PickCandidateIcon;
    public GameObject PickedIcon;

    public CardId CardId => this.card.Id;

    public bool IsPickCandidate => this.PickCandidateIcon.activeSelf || this.IsPicked;
    public bool IsPicked => this.PickedIcon.activeSelf;

    protected Card card;
    private bool shouldUpdate;

    // Update is called once per frame
    void Update()
    {
        if (this.card == null || !this.shouldUpdate)
        {
            return;
        }

        this.shouldUpdate = false;

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

        var (success, cardImageSprite) = CardImageCache.GetOrInit(this.card.Name);
        if (success)
        {
            this.CardImage.sprite = cardImageSprite;
        }
        else
        {
            // âÊëúÇ™Ç»Ç¢èÍçáÇæÇØñºëOÇï\é¶Ç∑ÇÈ
            this.CardNameText.text = this.card.Name;
            this.CardImage.enabled = false;
        }
    }

    public void SetCard(Card card)
    {
        this.card = card;
        this.shouldUpdate = true;
    }
}
