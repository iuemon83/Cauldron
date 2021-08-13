using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using DG.Tweening;
using System.Linq;
using System.Threading.Tasks;
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
    [SerializeField]
    private Image bounceDeckIconImage;
    [SerializeField]
    private Image bounceHandIconImage;

    public CardId CardId => this.Card.Id;

    public bool IsPickCandidate => this.PickCandidateIcon.activeSelf || this.IsPicked;
    public bool IsPicked => this.PickedIcon.activeSelf;

    public Card Card { get; private set; }

    private float timeElapsed;
    private int currentAbilityIndex;

    protected virtual void Update()
    {
        if (this.Card == null)
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
        if (this.Card.Abilities.Any())
        {
            this.abilityView.SetActive(true);

            this.currentAbilityIndex = this.currentAbilityIndex < this.Card.Abilities.Count - 1
                ? this.currentAbilityIndex + 1
                : 0;

            var (success, icon) = AbilityIconCache.TryGet(this.Card.Abilities[this.currentAbilityIndex]);
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
        this.Card = card;

        this.CostText.text = this.Card.Cost.ToString();

        switch (this.Card.Type)
        {
            case CardType.Creature:
                this.PowerText.text = this.Card.Power.ToString();
                this.ToughnessText.text = this.Card.Toughness.ToString();
                this.PowerSpace.SetActive(true);
                this.ToughnessSpace.SetActive(true);
                break;

            default:
                this.PowerSpace.SetActive(false);
                this.ToughnessSpace.SetActive(false);
                break;
        }

        var (success, cardImageSprite) = CardImageCache.GetOrInit(this.Card.Name);
        if (success)
        {
            this.CardImage.sprite = cardImageSprite;
        }
        else
        {
            // ‰æ‘œ‚ª‚È‚¢ê‡‚¾‚¯–¼‘O‚ð•\Ž¦‚·‚é
            this.CardNameText.text = this.Card.Name;
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

    public async Task BounceDeckEffect(PlayerController dest)
    {
        this.transform.SetAsLastSibling();

        await this.BounceDeckEffect(dest.transform.position);
    }

    public async Task BounceDeckEffect(Vector3 dest)
    {
        this.bounceDeckIconImage.gameObject.SetActive(true);
        await this.bounceDeckIconImage.transform
            .DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f)
            .ToAwaiter();

        var seq = DOTween.Sequence();
        await seq.Append(this.transform.DOMove(dest, 0.3f))
            .Join(this.transform.DORotate(new Vector3(0, 0, 1800), 0.3f, RotateMode.WorldAxisAdd))
            .ToAwaiter();

        this.bounceDeckIconImage.gameObject.SetActive(false);
    }

    public async Task BounceHandEffect(PlayerController dest)
    {
        this.transform.SetAsLastSibling();

        await this.BounceHandEffect(dest.transform.position);
    }

    public async Task BounceHandEffect(Vector3 dest)
    {
        this.bounceHandIconImage.gameObject.SetActive(true);
        await this.bounceHandIconImage.transform
            .DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f)
            .ToAwaiter();

        await DOTween.Sequence()
            .Append(this.transform.DOMove(dest, 0.3f))
            .Join(this.transform.DORotate(new Vector3(0, 0, 1800), 0.3f, RotateMode.WorldAxisAdd))
            .ToAwaiter();

        this.bounceHandIconImage.gameObject.SetActive(false);
    }

    public async Task ExcludeEffect()
    {
        await DOTween.Sequence()
            .Append(this.transform.DOScale(0f, 0.3f))
            .Join(this.transform.DORotate(new Vector3(0, 0, 1800), 0.3f, RotateMode.WorldAxisAdd))
            .ToAwaiter();
    }
}
