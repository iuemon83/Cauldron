using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardController : CardController, IPointerClickHandler
{
    [SerializeField]
    private Image destroyIcon = default;
    [SerializeField]
    private Image backgroundImage = default;
    [SerializeField]
    private Button playButton = default;

    private Action<Card> displaySmallCardDetail;
    Action<HandCardController> setPlayTargetHand;

    public void Init(Card card, Action<Card> displaySmallCardDetail, Action<HandCardController> setPlayTargetHand)
    {
        this.Init(card);

        this.displaySmallCardDetail = displaySmallCardDetail;
        this.setPlayTargetHand = setPlayTargetHand;
    }

    public  void OnPointerClick(PointerEventData eventData)
    {
        this.displaySmallCardDetail?.Invoke(this.Card);
        this.setPlayTargetHand?.Invoke(this);
    }

    public void DisablePlayTarget()
    {
        this.playButton.gameObject.SetActive(false);
    }

    public bool TogglePlayTarget()
    {
        this.playButton.gameObject.SetActive(!this.playButton.gameObject.activeSelf);
        return this.playButton.gameObject.activeSelf;
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

    public async void OnPlayButtonClick()
    {
        await BattleSceneController.Instance.PlayFromHand(this);
    }
}
