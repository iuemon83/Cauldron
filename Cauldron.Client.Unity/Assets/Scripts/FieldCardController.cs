using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class FieldCardController : CardController, IPointerClickHandler
{
    public GameObject SelectedIcon;
    public GameObject AttackTargetIcon;

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
            ClientController.Instance.PickedCardIdList.Remove(this.CardId);
        }
        else if (this.IsPickCandidate)
        {
            this.PickedIcon.SetActive(true);
            this.PickCandidateIcon.SetActive(false);
            ClientController.Instance.PickedCardIdList.Add(this.CardId);
        }
        else if (this.IsAttackTarget)
        {
            // ����̃J�[�h
            if (ClientController.Instance.SelectedCardController != null)
            {
                var attackCardId = ClientController.Instance.SelectedCardController.card.Id;
                var guardCardId = this.card.Id;

                // �U������
                await ClientController.Instance.Attack(attackCardId, guardCardId);

                // �U����͑I���ς݂̃J�[�h�̑I������������
                ClientController.Instance.UnSelectCard();
            }
        }
        else
        {
            // �����̃J�[�h

            var isSelected = this.SelectedIcon.activeSelf;

            if (ClientController.Instance.SelectedCardController != null)
            {
                // �J�[�h�I���ς�
                // �I������������
                this.HighlightLine.enabled = false;
                ClientController.Instance.UnSelectCard();
            }

            if (!isSelected)
            {
                //this.SetColor(CardStatus.Select);

                // ���I���̃J�[�h�Ȃ�I����Ԃɂ���
                await ClientController.Instance.SelectCard(this.CardId);
            }
        }
    }
}
