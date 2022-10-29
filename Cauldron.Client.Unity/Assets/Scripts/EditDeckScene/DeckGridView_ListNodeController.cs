using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckGridView_ListNodeController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Image cardIllustrationImage = default;
    [SerializeField]
    private Image maskImage = default;
    [SerializeField]
    private TextMeshProUGUI nameText = default;
    [SerializeField]
    private TextMeshProUGUI pickNumText = default;
    [SerializeField]
    private TextMeshProUGUI limitNumText = default;

    private CardDef source;

    private Action<CardDef> displayCardDetail;
    private Func<CardDefId, int> removeFromDeck;

    public string SortRank => this.source.Cost.ToString("000") + this.source.Name;

    public bool IsLimit => this.source.LimitNumCardsInDeck.Value == int.Parse(this.pickNumText.text);

    public bool OverLimit => this.source.LimitNumCardsInDeck.Value < int.Parse(this.pickNumText.text);

    public int PickNum => int.TryParse(this.pickNumText.text, out var intValue) ? intValue : 0;

    public bool IsEmpty => this.PickNum == 0;

    public void Init(
        CardDef source,
        Action<CardDef> displayCardDetail,
        Func<CardDefId, int> removeFromDeck
        )
    {
        this.source = source;
        this.displayCardDetail = displayCardDetail;
        this.removeFromDeck = removeFromDeck;

        var (success, cardImageSprite) = CardImageCache.GetOrInit(source.Name);
        if (success)
        {
            this.cardIllustrationImage.sprite = cardImageSprite;
            this.cardIllustrationImage.preserveAspect = true;
            this.cardIllustrationImage.gameObject.SetActive(true);
            this.nameText.gameObject.SetActive(false);
        }
        else
        {
            this.cardIllustrationImage.sprite = default;
            this.nameText.text = source.Name;
            this.cardIllustrationImage.gameObject.SetActive(false);
            this.nameText.gameObject.SetActive(true);
        }

        this.limitNumText.text = source.LimitNumCardsInDeck.Value.ToString();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            this.displayCardDetail?.Invoke(this.source);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // ƒfƒbƒL‚©‚ç”²‚­
            var actualRemoveNum = this.removeFromDeck(this.source.Id);
            if (actualRemoveNum > 0)
            {
                AudioController.CreateOrFind().PlayAudio(SeAudioCache.SeAudioType.Draw);
            }
            this.displayCardDetail?.Invoke(this.source);
        }
    }

    public void CountUp()
    {
        this.UpdateCurrentDeckCount(this.PickNum + 1);
    }

    public int CountDown()
    {
        if (this.PickNum == 0)
        {
            return 0;
        }

        this.UpdateCurrentDeckCount(this.PickNum - 1);
        return 1;
    }

    private void UpdateCurrentDeckCount(int x)
    {
        this.pickNumText.text = x.ToString();

        this.pickNumText.color = this.IsLimit
            ? EditDeckSceneController.PickNumTextColorLimit
            : this.OverLimit
                ? EditDeckSceneController.PickNumTextColorOver
                : EditDeckSceneController.PickNumTextColorNormal;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        this.maskImage.gameObject.SetActive(true);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        this.maskImage.gameObject.SetActive(false);
    }
}
