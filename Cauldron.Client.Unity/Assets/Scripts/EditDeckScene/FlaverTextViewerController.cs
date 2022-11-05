using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlaverTextViewerController : MonoBehaviour
{
    private static readonly string regexURL = "http(s)?://([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?";

    private static readonly string hyperLinkTag = "<color=#00ACBF><u><link=\"{0}\">{0}</link></u></color>";

    private static readonly CardAudioCache.CardAudioType[] CardAudioTypes
        = (CardAudioCache.CardAudioType[])Enum.GetValues(typeof(CardAudioCache.CardAudioType));

    [SerializeField]
    private TextMeshProUGUI cardNameText = default;

    [SerializeField]
    private Image cardIllustrationImage = default;

    [SerializeField]
    private TextMeshProUGUI flaverText = default;

    private CardDef source;
    private int currentIndex;

    private int CurrentCardAudioTypeIndex;

    private Func<int, (CardDef, int)> prevCard;
    private Func<int, (CardDef, int)> nextCard;

    public void Init(Func<int, (CardDef, int)> prevCard, Func<int, (CardDef, int)> nextCard)
    {
        this.prevCard = prevCard;
        this.nextCard = nextCard;
    }

    public void Open(CardDef cardDef, int cardPoolIndex)
    {
        this.source = cardDef;
        this.currentIndex = cardPoolIndex;
        this.CurrentCardAudioTypeIndex = 0;

        this.cardNameText.text = cardDef.Name;

        var (success, cardImageSprite) = CardImageCache.GetOrInit(cardDef.Name);
        if (success)
        {
            this.cardIllustrationImage.sprite = cardImageSprite;
            this.cardIllustrationImage.preserveAspect = true;
        }
        else
        {
            this.cardIllustrationImage.sprite = default;
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

    public async void OnPlaySoundButtonClick()
    {
        Debug.Log("on click PlaySoundButton");

        var done = false;
        var startIndex = this.CurrentCardAudioTypeIndex;
        // 1Žü‚µ‚Ä‚àSE‚ª–³‚©‚Á‚½‚ç‚â‚ß‚é
        do
        {
            done = await AudioController.CreateOrFind()
                .PlaySe2(this.source.Name, this.NextCardAudioType());
        }
        while (!done && startIndex != this.CurrentCardAudioTypeIndex);
    }

    private CardAudioCache.CardAudioType NextCardAudioType()
    {
        this.CurrentCardAudioTypeIndex++;
        if (this.CurrentCardAudioTypeIndex >= CardAudioTypes.Length)
        {
            this.CurrentCardAudioTypeIndex = 0;
        }

        return CardAudioTypes[this.CurrentCardAudioTypeIndex];
    }

    public void OnPrevButtonClick()
    {
        var (card, index) = this.prevCard(this.currentIndex);
        this.Open(card, index);
    }

    public void OnNextButtonClick()
    {
        var (card, index) = this.nextCard(this.currentIndex);
        this.Open(card, index);
    }
}
