using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class HandCardController : CardController, IPointerClickHandler, IPointerEnterHandler
{
    public async void OnPointerClick(PointerEventData eventData)
    {
        await ClientController.Instance.PlayFromHand(this.card.Id);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ClientController.Instance.CardDetailController.SetCard(this.card);
    }
}
