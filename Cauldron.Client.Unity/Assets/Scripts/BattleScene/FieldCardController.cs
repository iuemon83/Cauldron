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

    // Start is called before the first frame update
    void Start()
    {
        this.SelectedIcon.SetActive(false);
    }

    /// <summary>
    /// フィールドカードのクリックイベント
    /// </summary>
    /// <param name="eventData"></param>
    public async void OnPointerClick(PointerEventData eventData)
    {
        if (this.IsPicked)
        {
            this.PickCandidateIcon.SetActive(true);
            this.PickedIcon.SetActive(false);
            BattleSceneController.Instance.UnPick(this.CardId);
        }
        else if (this.IsPickCandidate)
        {
            this.PickedIcon.SetActive(true);
            this.PickCandidateIcon.SetActive(false);
            BattleSceneController.Instance.Pick(this.CardId);
        }
        else if (this.IsAttackTarget)
        {
            // 相手のカード
            if (BattleSceneController.Instance.SelectedCardController != null)
            {
                var attackCardId = BattleSceneController.Instance.SelectedCardController.card.Id;
                var guardCardId = this.card.Id;

                // 攻撃する
                await BattleSceneController.Instance.Attack(attackCardId, guardCardId);

                // 攻撃後は選択済みのカードの選択を解除する
                BattleSceneController.Instance.UnSelectCard();
            }
        }
        else
        {
            // 自分のカード

            var isSelected = this.SelectedIcon.activeSelf;

            if (BattleSceneController.Instance.SelectedCardController != null)
            {
                // カード選択済み
                // 選択を解除する
                BattleSceneController.Instance.UnSelectCard();
            }

            if (!isSelected)
            {
                //this.SetColor(CardStatus.Select);

                // 未選択のカードなら選択状態にする
                await BattleSceneController.Instance.SelectCard(this.CardId);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BattleSceneController.Instance.ShowCardDetail(this.card);
    }

    public void SetAttackTarget(bool value)
    {
        this.AttackTargetIcon.SetActive(value);
    }

    public void SetSelect(bool value)
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
}
