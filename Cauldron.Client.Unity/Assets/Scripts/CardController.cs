using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    //public enum CardStatus
    //{
    //    None,
    //    Select,
    //    Candidate,
    //    AttackTarget
    //}

    public Text CardName;

    public GameObject PowerSpace;
    public GameObject ToughnessSpace;

    public Text Cost;
    public Text Power;
    public Text Toughness;

    public GameObject PickCandidateIcon;
    public GameObject PickedIcon;

    public LineRenderer HighlightLine;

    public CardId CardId => this.card.Id;

    public bool IsPickCandidate => this.PickCandidateIcon.activeSelf || this.IsPicked;
    public bool IsPicked => this.PickedIcon.activeSelf;

    //public CardStatus Status { get; private set; }

    protected Card card;

    // Update is called once per frame
    void Update()
    {
        if (this.card != null)
        {
            this.CardName.text = this.card.Name;
            this.Cost.text = this.card.Cost.ToString();

            if (this.card.Type == CardType.Creature)
            {
                this.Power.text = this.card.Power.ToString();
                this.Toughness.text = this.card.Toughness.ToString();
            }
            else
            {
                this.PowerSpace.SetActive(false);
                this.ToughnessSpace.SetActive(false);
            }
        }
    }

    public void SetCard(Card card)
    {
        this.card = card;
    }

    //public void SetColor(CardStatus cardColor)
    //{
    //    var color = C(cardColor);
    //    this.HighlightLine.startColor = color;
    //    this.HighlightLine.endColor = color;

    //    this.HighlightLine.enabled = true;
    //}

    //public static Color C(CardStatus cardColor)
    //{
    //    return cardColor switch
    //    {
    //        CardStatus.Select => new Color(255, 0, 0),
    //        CardStatus.Candidate => new Color(0, 255, 0),
    //        CardStatus.AttackTarget => new Color(0, 0, 255),
    //        _ => new Color(),
    //    };
    //}

    //public void SetStatus(CardStatus cardStatus)
    //{
    //    this.Status = cardStatus;

    //    switch (cardStatus)
    //    {
    //        case CardStatus.Candidate:
    //            this.PickCandidateIcon.SetActive(true);
    //            break;

    //        default:
    //            this.PickCandidateIcon.SetActive(false);
    //            break;
    //    }
    //}
}
