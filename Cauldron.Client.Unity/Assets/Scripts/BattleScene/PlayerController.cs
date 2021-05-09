using Cauldron.Shared.MessagePackObjects;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI statusText;
    [SerializeField]
    private TextMeshProUGUI deckText;
    [SerializeField]
    private TextMeshProUGUI cemeteryText;
    [SerializeField]
    private TextMeshProUGUI handText;
    [SerializeField]
    private TextMeshProUGUI damageText;
    [SerializeField]
    private GameObject attackTargetIcon;
    [SerializeField]
    private GameObject pickCandidateIcon;
    [SerializeField]
    private GameObject pickedIcon;
    [SerializeField]
    private Image outline;

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

    private bool isActiveTurn;

    private float time;

    private void Update()
    {
        if (this.isActiveTurn)
        {
            this.time += Time.deltaTime * 5.0f;
            var color = this.outline.color;
            color.a = Mathf.Sin(this.time) * 0.5f + 0.5f;
            this.outline.color = color;
        }
    }

    public void Set(PublicPlayerInfo publicPlayerInfo)
    {
        this.PlayerId = publicPlayerInfo.Id;
        this.nameText.text = publicPlayerInfo.Name;
        this.statusText.text = $"[{publicPlayerInfo.CurrentHp} / {publicPlayerInfo.MaxHp}] [{publicPlayerInfo.CurrentMp} / {publicPlayerInfo.MaxMp}]";
        this.deckText.text = publicPlayerInfo.DeckCount.ToString();
        this.cemeteryText.text = publicPlayerInfo.Cemetery.Length.ToString();
        this.handText.text = publicPlayerInfo.HandsCount.ToString();
    }

    public void SetActiveTurn(bool value)
    {
        this.isActiveTurn = value;
        if (!this.isActiveTurn)
        {
            var color = this.outline.color;
            color.a = 0;
            this.outline.color = color;
        }
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
            BattleSceneController.Instance.UnPick(this.PlayerId);
        }
        else if (this.IsPickCandidate)
        {
            this.pickedIcon.SetActive(true);
            this.pickCandidateIcon.SetActive(false);
            BattleSceneController.Instance.Pick(this.PlayerId);
        }
        else if (this.IsAttackTarget)
        {
            if (BattleSceneController.Instance.SelectedCardController != null)
            {
                // 選択済みのカードがある

                var attackCardId = BattleSceneController.Instance.SelectedCardController.CardId;

                // 攻撃する
                await BattleSceneController.Instance.AttackToOpponentPlayer(attackCardId);

                // 攻撃後は選択済みのカードの選択を解除する
                BattleSceneController.Instance.UnSelectCard();
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
