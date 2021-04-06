using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    public Text CardName;

    public GameObject PowerSpace;
    public GameObject ToughnessSpace;

    public Text Cost;
    public Text Power;
    public Text Toughness;

    public SpriteRenderer CardImage;

    public GameObject PickCandidateIcon;
    public GameObject PickedIcon;

    public LineRenderer HighlightLine;

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

        var (success, cardImageSprite) = CardImageCache.GetOrInit(this.card.Name);
        if (success)
        {
            this.CardImage.sprite = cardImageSprite;
        }
        else
        {
            // âÊëúÇ™Ç»Ç¢èÍçáÇæÇØñºëOÇï\é¶Ç∑ÇÈ
            this.CardName.text = this.card.Name;
            this.CardImage.enabled = false;
        }
    }

    public void SetCard(Card card)
    {
        this.card = card;
        this.shouldUpdate = true;
    }
}
