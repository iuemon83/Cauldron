using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
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
    protected TextMeshProUGUI cardNameText = default;

    [SerializeField]
    protected TextMeshProUGUI costText = default;

    [SerializeField]
    protected GameObject powerSpace = default;
    [SerializeField]
    protected TextMeshProUGUI powerText = default;
    [SerializeField]
    protected GameObject toughnessSpace = default;
    [SerializeField]
    protected TextMeshProUGUI toughnessText = default;

    [SerializeField]
    protected GameObject[] counterSpaceList = Array.Empty<GameObject>();
    [SerializeField]
    protected TextMeshProUGUI[] counterTextList = Array.Empty<TextMeshProUGUI>();

    [SerializeField]
    protected Image cardImage = default;

    [SerializeField]
    protected GameObject pickCandidateIcon = default;
    [SerializeField]
    protected GameObject pickedIcon = default;
    [SerializeField]
    private GameObject abilityView = default;
    [SerializeField]
    private Image abilityIconImage = default;
    [SerializeField]
    private Image bounceDeckIconImage = default;
    [SerializeField]
    private Image bounceHandIconImage = default;

    public CardId CardId => this.Card.Id;

    public bool IsPickCandidate => this.pickCandidateIcon.activeSelf || this.IsPicked;
    public bool IsPicked => this.pickedIcon.activeSelf;

    public Card Card { get; private set; }

    protected virtual void Update()
    {
        if (this.Card == null)
        {
            return;
        }

        this.UpdateAbilityIcon();
    }

    private void UpdateAbilityIcon()
    {
        if (this.Card.Abilities.Any())
        {
            this.abilityView.SetActive(true);

            var index = (int)Math.Floor(Time.time) % this.Card.Abilities.Count;

            var (success, icon) = AbilityIconCache.TryGet(this.Card.Abilities[index]);
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

        this.costText.text = this.Card.Cost.ToString();

        switch (this.Card.Type)
        {
            case CardType.Creature:
                this.powerText.text = this.Card.Power.ToString();
                this.powerText.color = GetPowerToughnessTextColor(this.Card.BasePower, this.Card.Power);
                this.toughnessText.text = this.Card.Toughness.ToString();
                this.toughnessText.color = GetPowerToughnessTextColor(this.Card.BaseToughness, this.Card.Toughness);
                this.powerSpace.SetActive(true);
                this.toughnessSpace.SetActive(true);
                break;

            default:
                this.powerSpace.SetActive(false);
                this.toughnessSpace.SetActive(false);
                break;
        }

        var counterValues = this.Card.CountersByName.Values.ToArray();

        for (var i = 0; i < this.counterSpaceList.Length; i++)
        {
            if (counterValues.Length > i && counterValues[i] != 0)
            {
                this.counterSpaceList[i].SetActive(true);
                this.counterTextList[i].text = counterValues[i].ToString();
            }
            else
            {
                this.counterSpaceList[i].SetActive(false);
            }
        }

        var (success, cardImageSprite) = CardImageCache.GetOrInit(this.Card.Name);
        if (success)
        {
            this.cardImage.sprite = cardImageSprite;
        }
        else
        {
            // âÊëúÇ™Ç»Ç¢èÍçáÇæÇØñºëOÇï\é¶Ç∑ÇÈ
            this.cardNameText.text = this.Card.Name;
            this.cardImage.enabled = false;
        }

        this.UpdateAbilityIcon();
    }

    public void VisiblePickCandidateIcon(bool value)
    {
        this.pickCandidateIcon.SetActive(value);
    }

    public void VisiblePickedIcon(bool value)
    {
        this.pickedIcon.SetActive(value);
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
