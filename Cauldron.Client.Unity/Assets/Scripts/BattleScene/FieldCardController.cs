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
    /// �t�B�[���h�J�[�h�̃N���b�N�C�x���g
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
            // ����̃J�[�h
            if (BattleSceneController.Instance.SelectedCardController != null)
            {
                var attackCardId = BattleSceneController.Instance.SelectedCardController.card.Id;
                var guardCardId = this.card.Id;

                // �U������
                await BattleSceneController.Instance.Attack(attackCardId, guardCardId);

                // �U����͑I���ς݂̃J�[�h�̑I������������
                BattleSceneController.Instance.UnSelectCard();
            }
        }
        else
        {
            // �����̃J�[�h

            var isSelected = this.SelectedIcon.activeSelf;

            if (BattleSceneController.Instance.SelectedCardController != null)
            {
                // �J�[�h�I���ς�
                // �I������������
                BattleSceneController.Instance.UnSelectCard();
            }

            if (!isSelected)
            {
                //this.SetColor(CardStatus.Select);

                // ���I���̃J�[�h�Ȃ�I����Ԃɂ���
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
