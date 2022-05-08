using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FieldCardController : CardController, IPointerClickHandler
{
    [SerializeField]
    private GameObject selectedIcon = default;
    [SerializeField]
    private GameObject attackTargetIcon = default;
    [SerializeField]
    private TextMeshProUGUI damageText = default;
    [SerializeField]
    private Image outlineImage = default;
    [SerializeField]
    private Image backgroundImage = default;
    [SerializeField]
    private Image destroyIcon = default;

    private Action<Card> displaySmallCardDetail;

    public bool IsAttackTarget => this.attackTargetIcon.activeSelf;

    public void Init(Card card, Action<Card> displaySmallCardDetail)
    {
        this.Init(card);

        this.displaySmallCardDetail = displaySmallCardDetail;
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

    /// <summary>
    /// フィールドカードのクリックイベント
    /// </summary>
    /// <param name="eventData"></param>
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        this.displaySmallCardDetail?.Invoke(this.Card);

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (this.IsPicked)
            {
                BattleSceneController.Instance.UnPick(this);
            }
            else if (this.IsPickCandidate)
            {
                BattleSceneController.Instance.Pick(this);
            }
            else if (this.IsAttackTarget)
            {
                BattleSceneController.Instance.AttackToCardIfSelectedAttackCard(this);
            }
            else
            {
                // 自分のカード
                if (this.Card.OwnerId == BattleSceneController.Instance.YouId)
                {
                    BattleSceneController.Instance.ToggleAttackCard(this);
                }
            }
        }
    }

    public void VisibleAttackTargetIcon(bool value)
    {
        this.attackTargetIcon.SetActive(value);
    }

    public async void VisibleAttackIcon(bool value)
    {
        if (value)
        {
            await this.gameObject.transform.DOScale(1.2f, 0.3f);
        }
        else
        {
            await this.gameObject.transform.DOScale(1f, 0.3f);
        }
    }

    public async UniTask DamageEffect(int value)
    {
        this.damageText.text = value.ToString();
        this.damageText.gameObject.SetActive(true);
        await this.damageText.gameObject.transform
            .DOMove(new Vector3(0, -20, 0), 0.5f)
            .SetRelative(true);
        this.damageText.gameObject.SetActive(false);
        this.damageText.gameObject.transform.localPosition = Vector3.zero;
    }

    public async UniTask DestroyEffect()
    {
        this.destroyIcon.gameObject.SetActive(true);

        this.outlineImage.gameObject.SetActive(false);

        await DOTween.Sequence()
            .Append(this.destroyIcon.transform
                .DOScale(new Vector3(1.2f, 1.2f), 0.3f))
            .Join(DOTween
                .ToAlpha(
                    () => this.backgroundImage.color,
                    c => this.backgroundImage.color = c,
                    0f,
                    0.2f));

        this.destroyIcon.gameObject.SetActive(false);
    }

    public async UniTask AttackEffect(PlayerController dest)
    {
        var origIndex = this.transform.GetSiblingIndex();

        this.transform.SetAsLastSibling();

        await this.AttackEffect(dest.transform.position);

        this.transform.SetSiblingIndex(origIndex);
    }

    public async UniTask AttackEffect(FieldCardController dest)
    {
        var origIndex = this.transform.GetSiblingIndex();

        this.transform.SetAsLastSibling();

        await this.AttackEffect(dest.transform.position);

        this.transform.SetSiblingIndex(origIndex);
    }

    private async UniTask AttackEffect(Vector3 dest)
    {
        var destVec = Vector3.MoveTowards(
            this.transform.position,
            dest,
            (dest - this.transform.position).magnitude - 60);

        // まず拡大しながら後ろに下がる
        await DOTween.Sequence()
            .Append(this.transform.DOScale(1.2f, 0.2f))
            .Join(this.transform.DOMove(
                this.transform.position + (this.transform.position - dest).normalized * 70,
                0.2f));

        await DOTween.Sequence()
            // 相手の位置まで移動して、もとの位置に戻る
            .Append(this.transform
                .DOMove(destVec, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InQuart))
            // 元のサイズに戻りながら
            .Join(this.transform.DOScale(1f, 0.2f));
    }

    public override void ResetAllIcon()
    {
        base.ResetAllIcon();

        this.VisibleAttackIcon(false);
        this.VisibleAttackTargetIcon(false);
    }

    public void SetCanAttack(bool value)
    {
        this.outlineImage.gameObject.SetActive(value);
    }
}
