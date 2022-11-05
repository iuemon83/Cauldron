using Assets.Scripts;
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

    private Action displayCardDetail;
    private Func<CardDef, int> addDeck;

    public bool IsLimit => this.source.LimitNumCardsInDeck.Value == this.PickNum;

    public bool OverLimit => this.source.LimitNumCardsInDeck.Value < this.PickNum;

    public int PickNum => int.TryParse(this.pickNumText.text, out var intValue) ? intValue : 0;

    public void Init(
        CardDef source,
        int current,
        Action displayCardDetail,
        Func<CardDef, int> addDeck
        )
    {
        this.source = source;
        this.displayCardDetail = displayCardDetail;
        this.addDeck = addDeck;

        var (success, cardImageSprite) = CardImageCache.GetOrInit(source.Name);
        if (success)
        {
            // 画像が取れたら画像を出す
            this.cardIllustrationImage.sprite = cardImageSprite;
            this.cardIllustrationImage.preserveAspect = true;
            this.cardIllustrationImage.gameObject.SetActive(true);
            this.nameText.gameObject.SetActive(false);
        }
        else
        {
            // 画像がとれなかったらカード名を出す
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
            this.displayCardDetail?.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            var actualAddCount = this.addDeck?.Invoke(this.source) ?? 0;
            if (actualAddCount > 0)
            {
                AudioController.CreateOrFind().PlaySe(SeAudioCache.SeAudioType.Draw);
            }
            this.displayCardDetail?.Invoke();
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
