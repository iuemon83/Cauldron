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

    private Action<Card> openCardDetailView;

    public void Init(Card card, Action<Card> openCardDetailView)
    {
        this.Init(card);

        this.openCardDetailView = openCardDetailView;
    }

    public async void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            await BattleSceneController.Instance.PlayFromHand(this);
        }
        else
        {
            this.openCardDetailView?.Invoke(this.Card);
        }
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
