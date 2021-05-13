using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FieldCardController : CardController, IPointerClickHandler, IPointerEnterHandler
{
    [SerializeField]
    private GameObject SelectedIcon;
    [SerializeField]
    private GameObject AttackTargetIcon;
    [SerializeField]
    private TextMeshProUGUI DamageText;

    public bool IsAttackTarget => this.AttackTargetIcon.activeSelf;

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
        BattleSceneController.Instance.ShowCardDetail(this.card);
    }

    public void VisibleAttackTargetIcon(bool value)
    {
        this.AttackTargetIcon.SetActive(value);
    }

    public void VisibleAttackIcon(bool value)
    {
        this.SelectedIcon.SetActive(value);
    }

    public async Task DamageEffect(int value)
    {
        this.DamageText.text = value.ToString();
        this.DamageText.gameObject.SetActive(true);
        await Task.Delay(TimeSpan.FromSeconds(0.3));
        this.DamageText.gameObject.SetActive(false);
    }

    public override void ResetAllIcon()
    {
        base.ResetAllIcon();

        this.VisibleAttackIcon(false);
        this.VisibleAttackTargetIcon(false);
    }
}
