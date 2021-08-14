using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI firstText;
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI statusText;
    [SerializeField]
    private TextMeshProUGUI numDecksText;
    [SerializeField]
    private TextMeshProUGUI numCemeteriesText;
    [SerializeField]
    private TextMeshProUGUI numHandsText;
    [SerializeField]
    private TextMeshProUGUI numExcludedsText;
    [SerializeField]
    private TextMeshProUGUI damageText;
    [SerializeField]
    private TextMeshProUGUI healText;
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
        this.numDecksText.text = publicPlayerInfo.DeckCount.ToString();
        this.numCemeteriesText.text = publicPlayerInfo.Cemetery.Length.ToString();
        this.numExcludedsText.text = publicPlayerInfo.Excluded.Length.ToString();
        this.numHandsText.text = publicPlayerInfo.HandsCount.ToString();
        this.firstText.gameObject.SetActive(publicPlayerInfo.IsFirst);
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
            BattleSceneController.Instance.UnPick(this);
        }
        else if (this.IsPickCandidate)
        {
            BattleSceneController.Instance.Pick(this);
        }
        else if (this.IsAttackTarget)
        {
            // 攻撃する
            await BattleSceneController.Instance.AttackToOpponentPlayerIfSelectedAttackCard();
        }
    }

    public void VisibleAttackTargetIcon(bool value)
    {
        this.attackTargetIcon.SetActive(value);
    }

    public void VisiblePickCandidateIcon(bool value)
    {
        this.pickCandidateIcon.SetActive(value);
    }

    public void VisiblePickedIcon(bool value)
    {
        this.pickedIcon.SetActive(value);
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

    public async UniTask HealEffect(int value)
    {
        this.healText.text = value.ToString();
        this.healText.gameObject.SetActive(true);
        await this.healText.gameObject.transform
            .DOMove(new Vector3(0, 20, 0), 0.5f)
            .SetRelative(true);
        this.healText.gameObject.SetActive(false);
        this.healText.gameObject.transform.localPosition = Vector3.zero;
    }

    public virtual void ResetAllIcon()
    {
        this.VisibleAttackTargetIcon(false);
        this.VisiblePickCandidateIcon(false);
        this.VisiblePickedIcon(false);
    }
}
