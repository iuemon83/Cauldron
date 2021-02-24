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
    /// フィールドカードのクリックイベント
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
            // 相手のカード
            if (ClientController.Instance.SelectedCardController != null)
            {
                var attackCardId = ClientController.Instance.SelectedCardController.card.Id;
                var guardCardId = this.card.Id;

                // 攻撃する
                await ClientController.Instance.Attack(attackCardId, guardCardId);

                // 攻撃後は選択済みのカードの選択を解除する
                ClientController.Instance.UnSelectCard();
            }
        }
        else
        {
            // 自分のカード

            var isSelected = this.SelectedIcon.activeSelf;

            if (ClientController.Instance.SelectedCardController != null)
            {
                // カード選択済み
                // 選択を解除する
                this.HighlightLine.enabled = false;
                ClientController.Instance.UnSelectCard();
            }

            if (!isSelected)
            {
                //this.SetColor(CardStatus.Select);

                // 未選択のカードなら選択状態にする
                await ClientController.Instance.SelectCard(this.CardId);
            }
        }
    }
}
