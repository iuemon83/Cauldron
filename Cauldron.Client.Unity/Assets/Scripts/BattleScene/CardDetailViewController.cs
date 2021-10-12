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
            ? "なし"
            : string.Join(Environment.NewLine, card.CountersByName.Select(x => $"{x.Key}: {x.Value}"));

        var result = @$"攻撃可能な回数: {card.NumAttacksLimitInTurn}
場に出てからのターン数: {card.NumTurnsInField}
攻撃可能となるまでのターン数: {card.NumTurnsToCanAttack}
カウンター：
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
