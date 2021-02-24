using Cauldron.Shared.MessagePackObjects;
using UnityEngine;
using UnityEngine.EventSystems;

//[RequireComponent(typeof(Rigidbody2D))]
public class OpponentPlayerController : MonoBehaviour, IPointerClickHandler
{
    public GameObject AttackTargetIcon;
    public GameObject PickCandidateIcon;
    public GameObject PickedIcon;

    public PlayerId PlayerId { get; set; }

    public bool IsPickCandidate => this.PickCandidateIcon.activeSelf || this.IsPicked;
    public bool IsPicked => this.PickedIcon.activeSelf;

    public bool IsAttackTarget
    {
        get => this.AttackTargetIcon?.activeSelf ?? false;
        set
        {
            this.AttackTargetIcon.SetActive(value);
        }
    }

    /// <summary>
    /// �G�v���C���[�A�C�R���̃N���b�N�C�x���g
    /// </summary>
    /// <param name="eventData"></param>
    public async void OnPointerClick(PointerEventData eventData)
    {
        if (this.IsPicked)
        {
            this.PickCandidateIcon.SetActive(true);
            this.PickedIcon.SetActive(false);
            ClientController.Instance.PickedPlayerIdList.Remove(this.PlayerId);
        }
        else if (this.IsPickCandidate)
        {
            this.PickedIcon.SetActive(true);
            this.PickCandidateIcon.SetActive(false);
            ClientController.Instance.PickedPlayerIdList.Add(this.PlayerId);
        }
        else if (this.IsAttackTarget)
        {
            if (ClientController.Instance.SelectedCardController != null)
            {
                // �I���ς݂̃J�[�h������

                var attackCardId = ClientController.Instance.SelectedCardController.CardId;

                // �U������
                await ClientController.Instance.AttackToOpponentPlayer(attackCardId);

                // �U����͑I���ς݂̃J�[�h�̑I������������
                ClientController.Instance.UnSelectCard();
            }
        }
    }
}
