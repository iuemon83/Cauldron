using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ReadonlyCardListView_ListNodeController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI cardNameText = default;

    private Action<Card> showCardDetailAction;

    private Card card;

    public void Init(Card card, Action<Card> ShowDetailAction)
    {
        this.card = card;
        this.showCardDetailAction = ShowDetailAction;

        this.cardNameText.text = this.card?.Name ?? "";
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            this.showCardDetailAction(this.card);
        }
    }
}
