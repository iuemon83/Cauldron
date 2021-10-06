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
        var result = "";

        if (card.CountersByName.Count != 0)
        {
            result += @$"カウンター
{string.Join(Environment.NewLine, card.CountersByName.Select(x => $"{x.Key}: {x.Value}"))}
";
        }

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
