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
        var counterText = card.CountersByName.Count == 0
            ? "�Ȃ�"
            : string.Join(Environment.NewLine, card.CountersByName.Select(x => $"{x.Key}: {x.Value}"));

        var result = @$"�U���\�ȉ�: {card.NumAttacksLimitInTurn}
��ɏo�Ă���̃^�[����: {card.NumTurnsInField}
�U���\�ƂȂ�܂ł̃^�[����: {card.NumTurnsToCanAttack}
�J�E���^�[�F
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
