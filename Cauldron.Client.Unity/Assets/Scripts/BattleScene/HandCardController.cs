using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardController : CardController, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField]
    private Image destroyIcon;

    public async void OnPointerClick(PointerEventData eventData)
    {
        await BattleSceneController.Instance.PlayFromHand(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BattleSceneController.Instance.ShowCardDetail(this.Card);
    }

    public async Task DestroyEffect()
    {
        this.destroyIcon.gameObject.SetActive(true);
        await this.destroyIcon.transform
            .DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f)
            .ToAwaiter();
        this.destroyIcon.gameObject.SetActive(false);
    }
}
