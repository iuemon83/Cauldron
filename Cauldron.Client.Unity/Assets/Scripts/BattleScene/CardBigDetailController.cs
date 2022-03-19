using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardBigDetailController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI cardNameText = default;

    [SerializeField]
    private Image CardIllustrationImage = default;

    [SerializeField]
    private TextMeshProUGUI otherDetailText = default;

    [SerializeField]
    private TextMeshProUGUI effectText = default;

    private string OtherText(Card card)
    {
        var annnotationsText = card.Annotations.Count == 0
            ? "�Ȃ�"
            : string.Join(",", card.Annotations);

        var result =
$@"{card.Cost} / {card.Power} / {card.Toughness}
�A�m�e�[�V���� | {annnotationsText}";

        if (card.Type == CardType.Creature)
        {
            result += Environment.NewLine +
$@"�U���� | {card.NumAttacksLimitInTurn}
�U���\�܂ł̃^�[�� | {card.NumTurnsToCanAttack}";
        }

        if (card.Zone.ZoneName == ZoneName.Field)
        {
            result += Environment.NewLine +
$@"��ɏo�Ă���̃^�[���� | {card.NumTurnsInField + 1}";
        }

        var counterText = card.CountersByName.Count == 0
            ? "�Ȃ�"
            : string.Join(Environment.NewLine, card.CountersByName.Select(x => $"{x.Key}: {x.Value}"));

        result += Environment.NewLine +
$@"�J�E���^�[ | {counterText}";

        return result;
    }

    public void Open(Card card)
    {
        this.cardNameText.text = card.Name;

        var (success, cardImageSprite) = CardImageCache.GetOrInit(card.Name);
        if (success)
        {
            this.CardIllustrationImage.sprite = cardImageSprite;
        }
        else
        {
            this.CardIllustrationImage.sprite = default;
        }

        this.otherDetailText.text = this.OtherText(card);
        this.effectText.text = card.EffectDescription;

        this.gameObject.SetActive(true);
    }

    public void Open(CardDef cardDef)
    {
        this.cardNameText.text = cardDef.Name;

        var (success, cardImageSprite) = CardImageCache.GetOrInit(cardDef.Name);
        if (success)
        {
            this.CardIllustrationImage.sprite = cardImageSprite;
        }
        else
        {
            this.CardIllustrationImage.sprite = default;
        }

        this.otherDetailText.text = "";
        this.effectText.text = cardDef.EffectDescription;

        this.gameObject.SetActive(true);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
}
