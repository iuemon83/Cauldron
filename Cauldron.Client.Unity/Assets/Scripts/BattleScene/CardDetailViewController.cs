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
            ? "なし"
            : string.Join(Environment.NewLine, card.Annotations.Select(x => $"・{x}"));

        var result =
$@"詳細
アノテーション:
{annnotationsText}";

        if (card.Type == CardType.Creature)
        {
            result +=
$@"
攻撃回数:
{card.NumAttacksLimitInTurn}
攻撃可能までのターン:
{card.NumTurnsToCanAttack}";
        }

        if (card.Zone.ZoneName == ZoneName.Field)
        {
            result +=
$@"
場に出てからのターン数:
{card.NumTurnsInField + 1}";
        }

        var counterText = card.CountersByName.Count == 0
            ? "なし"
            : string.Join(Environment.NewLine, card.CountersByName.Select(x => $"{x.Key}: {x.Value}"));

        result +=
$@"
カウンター:
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
