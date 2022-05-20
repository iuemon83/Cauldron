using Cauldron.Shared.MessagePackObjects;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FlaverTextViewerController : MonoBehaviour
{
    private static readonly string regexURL = "http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?";

    private static readonly string hyperLinkTag = "<color=#00ACBF><u><link=\"{0}\">{0}</link></u></color>";

    [SerializeField]
    private TextMeshProUGUI cardNameText = default;

    [SerializeField]
    private Image CardIllustrationImage = default;

    [SerializeField]
    private TextMeshProUGUI flaverText = default;

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

        this.flaverText.text = FlaverText(cardDef);

        this.gameObject.SetActive(true);
    }

    private string FlaverText(CardDef cardDef)
    {
        var flaverText = cardDef.FlavorText;

        var matches = Regex.Matches(cardDef.FlavorText, regexURL);

        foreach (Match match in matches)
        {
            flaverText = flaverText.Replace(match.Value, string.Format(hyperLinkTag, match.Value));
        }

        return flaverText;
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
}
