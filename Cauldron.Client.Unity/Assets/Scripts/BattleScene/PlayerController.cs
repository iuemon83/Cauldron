using Cauldron.Shared.MessagePackObjects;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text statusText;
    [SerializeField]
    private Text deckText;
    [SerializeField]
    private Text cemeteryText;
    [SerializeField]
    private TextMeshProUGUI damageText;
    [SerializeField]
    private GameObject attackTargetIcon;
    [SerializeField]
    private GameObject pickCandidateIcon;
    [SerializeField]
    private GameObject pickedIcon;

    public PlayerId PlayerId { get; private set; }

    public bool IsPickCandidate => this.pickCandidateIcon.activeSelf || this.IsPicked;
    public bool IsPicked => this.pickedIcon.activeSelf;

    public bool IsAttackTarget
    {
        get => this.attackTargetIcon.activeSelf;
        set
        {
            this.attackTargetIcon.SetActive(value);
        }
    }

    public void Set(PublicPlayerInfo publicPlayerInfo)
    {
        this.PlayerId = publicPlayerInfo.Id;
        this.nameText.text = publicPlayerInfo.Name;
        this.statusText.text = $"[{publicPlayerInfo.CurrentHp} / {publicPlayerInfo.MaxHp}] [{publicPlayerInfo.CurrentMp} / {publicPlayerInfo.MaxMp}]";
        this.deckText.text = publicPlayerInfo.DeckCount.ToString();
        this.cemeteryText.text = publicPlayerInfo.Cemetery.Length.ToString();
    }

    /// <summary>
    /// 敵プレイヤーアイコンのクリックイベント
    /// </summary>
    /// <param name="eventData"></param>
    public async void OnPointerClick(PointerEventData eventData)
    {
        if (this.IsPicked)
        {
            this.pickCandidateIcon.SetActive(true);
            this.pickedIcon.SetActive(false);
            ClientController.Instance.PickedPlayerIdList.Remove(this.PlayerId);
        }
        else if (this.IsPickCandidate)
        {
            this.pickedIcon.SetActive(true);
            this.pickCandidateIcon.SetActive(false);
            ClientController.Instance.PickedPlayerIdList.Add(this.PlayerId);
        }
        else if (this.IsAttackTarget)
        {
            if (ClientController.Instance.SelectedCardController != null)
            {
                // 選択済みのカードがある

                var attackCardId = ClientController.Instance.SelectedCardController.CardId;

                // 攻撃する
                await ClientController.Instance.AttackToOpponentPlayer(attackCardId);

                // 攻撃後は選択済みのカードの選択を解除する
                ClientController.Instance.UnSelectCard();
            }
        }
    }

    public void SetAttackTarget(bool value)
    {
        this.attackTargetIcon.SetActive(value);
    }

    public void SetPickeCandidate(bool value)
    {
        this.pickCandidateIcon.SetActive(value);
    }

    public void SetPicked(bool value)
    {
        this.pickedIcon.SetActive(value);
    }

    public async void DamageEffect(int value)
    {
        this.damageText.text = value.ToString();
        this.damageText.gameObject.SetActive(true);
        await Task.Delay(500);
        this.damageText.gameObject.SetActive(false);
    }
}
