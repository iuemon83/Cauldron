using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class CardDetailViewController : MonoBehaviour
{
    [SerializeField]
    private CardDetailController cardDetailController = default;

    [SerializeField]
    private TextMeshProUGUI otherDetailText = default;

    private string OtherText(Card card)
    {
        var annnotationsText = card.Annotations.Count == 0
            ? "�Ȃ�"
            : string.Join(Environment.NewLine, card.Annotations.Select(x => $"�E{x}"));

        var result =
$@"�ڍ�
�A�m�e�[�V����:
{annnotationsText}";

        if (card.Type == CardType.Creature)
        {
            result +=
$@"
�U����:
{card.NumAttacksLimitInTurn}
�U���\�܂ł̃^�[��:
{card.NumTurnsToCanAttack}";
        }

        if (card.Zone.ZoneName == ZoneName.Field)
        {
            result +=
$@"
��ɏo�Ă���̃^�[����:
{card.NumTurnsInField + 1}";
        }

        var counterText = card.CountersByName.Count == 0
            ? "�Ȃ�"
            : string.Join(Environment.NewLine, card.CountersByName.Select(x => $"{x.Key}: {x.Value}"));

        result +=
$@"
�J�E���^�[:
{counterText}";

        return result;
    }

    public void Open(Card card)
    {
        this.cardDetailController.SetCard(card);

        this.otherDetailText.text = this.OtherText(card);

        this.gameObject.SetActive(true);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
}
