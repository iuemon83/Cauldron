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

    public GameObject PickCandidateIcon;
    public GameObject PickedIcon;

    public LineRenderer HighlightLine;

    public CardId CardId => this.card.Id;

    public bool IsPickCandidate => this.PickCandidateIcon.activeSelf || this.IsPicked;
    public bool IsPicked => this.PickedIcon.activeSelf;

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
                this.PowerSpace.SetActive(true);
                this.ToughnessSpace.SetActive(true);
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
}
