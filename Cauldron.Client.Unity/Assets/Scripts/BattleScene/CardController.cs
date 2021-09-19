using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    private static Color GetPowerToughnessTextColor(int baseValue, int actualValue)
    {
        return baseValue == actualValue
            ? Color.white
            : baseValue > actualValue
                ? Color.red
                : Color.green;
    }

    [SerializeField]
    protected TextMeshProUGUI CardNameText = default;

    [SerializeField]
    protected GameObject PowerSpace = default;
    [SerializeField]
    protected GameObject ToughnessSpace = default;

    [SerializeField]
    protected TextMeshProUGUI CostText = default;
    [SerializeField]
    protected TextMeshProUGUI PowerText = default;
    [SerializeField]
    protected TextMeshProUGUI ToughnessText = default;

    [SerializeField]
    protected Image CardImage = default;

    [SerializeField]
    protected GameObject PickCandidateIcon = default;
    [SerializeField]
    protected GameObject PickedIcon = default;
    [SerializeField]
    private GameObject abilityView = default;
    [SerializeField]
    private Image abilityIconImage = default;
    [SerializeField]
    private Image bounceDeckIconImage = default;
    [SerializeField]
    private Image bounceHandIconImage = default;

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
                this.PowerText.color = GetPowerToughnessTextColor(this.Card.BasePower, this.Card.Power);
                this.ToughnessText.text = this.Card.Toughness.ToString();
                this.ToughnessText.color = GetPowerToughnessTextColor(this.Card.BaseToughness, this.Card.Toughness);
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

    public async UniTask BounceDeckEffect(PlayerController dest)
    {
        this.transform.SetAsLastSibling();

        await this.BounceDeckEffect(dest.transform.position);
    }

    public async UniTask BounceDeckEffect(Vector3 dest)
    {
        this.bounceDeckIconImage.gameObject.SetActive(true);
        await this.bounceDeckIconImage.transform
            .DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f);

        var seq = DOTween.Sequence();
        await seq.Append(this.transform.DOMove(dest, 0.3f))
            .Join(this.transform.DORotate(new Vector3(0, 0, 1800), 0.3f, RotateMode.WorldAxisAdd));

        this.bounceDeckIconImage.gameObject.SetActive(false);
    }

    public async UniTask BounceHandEffect(PlayerController dest)
    {
        this.transform.SetAsLastSibling();

        await this.BounceHandEffect(dest.transform.position);
    }

    public async UniTask BounceHandEffect(Vector3 dest)
    {
        this.bounceHandIconImage.gameObject.SetActive(true);
        await this.bounceHandIconImage.transform
            .DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f);

        await DOTween.Sequence()
            .Append(this.transform.DOMove(dest, 0.3f))
            .Join(this.transform.DORotate(new Vector3(0, 0, 1800), 0.3f, RotateMode.WorldAxisAdd));

        this.bounceHandIconImage.gameObject.SetActive(false);
    }

    public async UniTask ExcludeEffect()
    {
        await DOTween.Sequence()
            .Append(this.transform.DOScale(0f, 0.3f))
            .Join(this.transform.DORotate(new Vector3(0, 0, 1800), 0.3f, RotateMode.WorldAxisAdd));
    }
}
