using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [SerializeField]
    protected TextMeshProUGUI CardNameText;

    [SerializeField]
    protected GameObject PowerSpace;
    [SerializeField]
    protected GameObject ToughnessSpace;

    [SerializeField]
    protected TextMeshProUGUI CostText;
    [SerializeField]
    protected TextMeshProUGUI PowerText;
    [SerializeField]
    protected TextMeshProUGUI ToughnessText;

    [SerializeField]
    protected Image CardImage;

    [SerializeField]
    protected GameObject PickCandidateIcon;
    [SerializeField]
    protected GameObject PickedIcon;
    [SerializeField]
    private GameObject abilityView;
    [SerializeField]
    private Image abilityIconImage;

    public CardId CardId => this.card.Id;

    public bool IsPickCandidate => this.PickCandidateIcon.activeSelf || this.IsPicked;
    public bool IsPicked => this.PickedIcon.activeSelf;

    protected Card card;

    private float timeElapsed;
    private int currentAbilityIndex;

    protected virtual void Update()
    {
        if (this.card == null)
        {
            return;
        }

        this.timeElapsed += Time.deltaTime;
        if (this.timeElapsed >= 1f)
        {
            this.UpdateAbilityIcon();
            this.timeElapsed = 0f;
        }
    }

    private void UpdateAbilityIcon()
    {
        if (this.card.Abilities.Any())
        {
            this.abilityView.SetActive(true);

            this.currentAbilityIndex = this.currentAbilityIndex == this.card.Abilities.Count - 1
                ? 0
                : this.currentAbilityIndex + 1;

            var (success, icon) = AbilityIconCache.TryGet(this.card.Abilities[this.currentAbilityIndex]);
            if (success)
            {
                this.abilityIconImage.sprite = icon;
            }
        }
        else
        {
            this.abilityView.SetActive(false);
        }
    }

    public virtual void Init(Card card)
    {
        this.card = card;

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

        this.UpdateAbilityIcon();
    }

    public void VisiblePickCandidateIcon(bool value)
    {
        this.PickCandidateIcon.SetActive(value);
    }

    public void VisiblePickedIcon(bool value)
    {
        this.PickedIcon.SetActive(value);
    }

    public virtual void ResetAllIcon()
    {
        this.VisiblePickCandidateIcon(false);
        this.VisiblePickedIcon(false);
    }
}
