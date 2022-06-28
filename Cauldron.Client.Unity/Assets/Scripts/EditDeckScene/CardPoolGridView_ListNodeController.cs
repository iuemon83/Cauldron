using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardPoolGridView_ListNodeController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
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
    private Func<CardDef, int> addDeck;

    public bool IsLimit => this.source.LimitNumCardsInDeck.Value == this.PickNum;

    public bool OverLimit => this.source.LimitNumCardsInDeck.Value < this.PickNum;

    public int PickNum => int.TryParse(this.pickNumText.text, out var intValue) ? intValue : 0;

    public void Init(
        CardDef source,
        int current,
        Action<CardDef> displayCardDetail,
        Func<CardDef, int> addDeck
        )
    {
        this.source = source;
        this.displayCardDetail = displayCardDetail;
        this.addDeck = addDeck;

        var (success, cardImageSprite) = CardImageCache.GetOrInit(source.Name);
        if (success)
        {
            this.cardIllustrationImage.sprite = cardImageSprite;
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

        this.UpdateCurrentDeckCount(current);
        this.limitNumText.text = source.LimitNumCardsInDeck.Value.ToString();

        if (source.IsToken)
        {
            this.limitNumText.color = EditDeckSceneController.LimitNumTextColorToken;
        }
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            this.displayCardDetail?.Invoke(this.source);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            var actualAddCount = this.addDeck?.Invoke(this.source) ?? 0;
            this.displayCardDetail?.Invoke(this.source);
        }
    }

    public int DeckCountUp()
    {
        this.UpdateCurrentDeckCount(this.PickNum + 1);
        return 1;
    }

    public int DeckCountDown()
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
