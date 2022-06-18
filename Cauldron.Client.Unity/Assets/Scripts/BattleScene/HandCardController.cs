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
    private Image outlineImage = default;
    [SerializeField]
    private Image backgroundImage = default;
    [SerializeField]
    private Button playButton = default;

    private Action<Card> displaySmallCardDetail;
    private Action<HandCardController> unPick;
    private Action<HandCardController> pick;
    private Action<HandCardController> setPlayTargetHand;

    public void Init(Card card, Action<Card> displaySmallCardDetail,
        Action<HandCardController> unPick,
        Action<HandCardController> pick,
        Action<HandCardController> setPlayTargetHand)
    {
        this.Init(card);

        this.displaySmallCardDetail = displaySmallCardDetail;
        this.unPick = unPick;
        this.pick = pick;
        this.setPlayTargetHand = setPlayTargetHand;
    }

    protected override void Update()
    {
        base.Update();

        this.UpdateOutlineColorByTime();
    }

    public void UpdateOutlineColorByTime()
    {
        var color = this.outlineImage.color;
        color.a = Mathf.Sin(2 * Mathf.PI * 0.5f * Time.time) * 0.5f + 0.5f;
        this.outlineImage.color = color;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        this.displaySmallCardDetail?.Invoke(this.Card);

        if (this.IsPicked)
        {
            this.unPick(this);
        }
        else if (this.IsPickCandidate)
        {
            this.pick(this);
        }
        else
        {
            this.setPlayTargetHand?.Invoke(this);
        }
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

    public void SetCanPlay(bool value)
    {
        this.outlineImage.gameObject.SetActive(value);
    }
}
