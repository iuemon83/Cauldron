using UnityEngine.EventSystems;

public class HandCardController : CardController, IPointerClickHandler, IPointerEnterHandler
{
    public async void OnPointerClick(PointerEventData eventData)
    {
        await BattleSceneController.Instance.PlayFromHand(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BattleSceneController.Instance.ShowCardDetail(this.card);
    }
}
