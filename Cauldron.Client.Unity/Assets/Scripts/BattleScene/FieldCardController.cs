using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FieldCardController : CardController, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField]
    private GameObject selectedIcon;
    [SerializeField]
    private GameObject attackTargetIcon;
    [SerializeField]
    private TextMeshProUGUI damageText;
    [SerializeField]
    private Image destroyIcon;

    public bool IsAttackTarget => this.attackTargetIcon.activeSelf;

    /// <summary>
    /// フィールドカードのクリックイベント
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
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
            BattleSceneController.Instance.SetAttackCard(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BattleSceneController.Instance.ShowCardDetail(this.Card);
    }

    public void VisibleAttackTargetIcon(bool value)
    {
        this.attackTargetIcon.SetActive(value);
    }

    public void VisibleAttackIcon(bool value)
    {
        this.selectedIcon.SetActive(value);
    }

    public async Task DamageEffect(int value)
    {
        this.damageText.text = value.ToString();
        this.damageText.gameObject.SetActive(true);
        await this.damageText.gameObject.transform
            .DOMove(new Vector3(0, -20, 0), 0.5f)
            .SetRelative(true)
            .ToAwaiter();
        this.damageText.gameObject.SetActive(false);
    }

    public async Task DestroyEffect()
    {
        this.destroyIcon.gameObject.SetActive(true);
        await this.destroyIcon.transform
            .DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f)
            .ToAwaiter();
        this.destroyIcon.gameObject.SetActive(false);
    }

    public async Task AttackEffect(PlayerController dest)
    {
        var origIndex = this.transform.GetSiblingIndex();

        this.transform.SetAsLastSibling();

        await this.AttackEffect(dest.transform.position);

        this.transform.SetSiblingIndex(origIndex);
    }

    public async Task AttackEffect(FieldCardController dest)
    {
        var origIndex = this.transform.GetSiblingIndex();

        this.transform.SetAsLastSibling();

        await this.AttackEffect(dest.transform.position);

        this.transform.SetSiblingIndex(origIndex);
    }

    public async Task AttackEffect(Vector3 dest)
    {
        await this.transform
            .DOScale(1.2f, 0.2f)
            .ToAwaiter();

        await this.transform
            .DOMove(dest, 0.2f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.InQuart)
            .ToAwaiter();

        await this.transform
            .DOScale(1f, 0f)
            .ToAwaiter();
    }

    public override void ResetAllIcon()
    {
        base.ResetAllIcon();

        this.VisibleAttackIcon(false);
        this.VisibleAttackTargetIcon(false);
    }
}
