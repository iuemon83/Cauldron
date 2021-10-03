using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardController : CardController, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField]
    private Image destroyIcon = default;
    [SerializeField]
    private Image backgroundImage = default;

    public async void OnPointerClick(PointerEventData eventData)
    {
        await BattleSceneController.Instance.PlayFromHand(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //BattleSceneController.Instance.ShowCardDetail(this.Card);
    }

    public async UniTask DestroyEffect()
    {
        this.destroyIcon.gameObject.SetActive(true);

        await DOTween.Sequence()
            .Append(this.destroyIcon.transform
                .DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f))
            .Join(DOTween
                .ToAlpha(
                    () => this.backgroundImage.color,
                    c => this.backgroundImage.color = c,
                    0f,
                    0.2f));

        this.destroyIcon.gameObject.SetActive(false);
    }
}
