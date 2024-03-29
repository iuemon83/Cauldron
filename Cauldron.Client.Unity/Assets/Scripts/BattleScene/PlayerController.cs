using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private GameObject playerIcon = default;
    [SerializeField]
    private TextMeshProUGUI nameText = default;
    [SerializeField]
    private TextMeshProUGUI hpText = default;
    [SerializeField]
    private TextMeshProUGUI mpText = default;
    [SerializeField]
    private TextMeshProUGUI numDecksText = default;
    [SerializeField]
    private TextMeshProUGUI numCemeteriesText = default;
    [SerializeField]
    private TextMeshProUGUI numHandsText = default;
    [SerializeField]
    private TextMeshProUGUI numExcludedsText = default;
    [SerializeField]
    private TextMeshProUGUI damageText = default;
    [SerializeField]
    private TextMeshProUGUI healText = default;
    [SerializeField]
    private GameObject attackTargetIcon = default;
    [SerializeField]
    private GameObject pickCandidateIcon = default;
    [SerializeField]
    private GameObject pickedIcon = default;
    [SerializeField]
    private Image outlineImage = default;

    public PlayerId PlayerId { get; private set; }

    private Action<PlayerController> unPick;
    private Action<PlayerController> pick;

    public Vector3 AttackDestPosition => this.playerIcon.transform.position;

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

    private void Update()
    {
        this.UpdateOutlineColorByTime();
    }

    private void UpdateOutlineColorByTime()
    {
        var color = this.outlineImage.color;
        color.a = Mathf.Sin(2 * Mathf.PI * 0.5f * Time.time) * 0.5f + 0.5f;
        this.outlineImage.color = color;
    }

    public void Init(
        Action<PlayerController> unPick,
        Action<PlayerController> pick
        )
    {
        this.unPick = unPick;
        this.pick = pick;
    }

    public void Set(PlayerId playerId)
    {
        this.PlayerId = playerId;
    }

    public void Set(PublicPlayerInfo publicPlayerInfo)
    {
        this.PlayerId = publicPlayerInfo.Id;
        this.nameText.text = publicPlayerInfo.Name;
        this.hpText.text = publicPlayerInfo.CurrentHp.ToString();
        this.mpText.text = publicPlayerInfo.CurrentMp.ToString();
        this.numDecksText.text = publicPlayerInfo.DeckCount.ToString();
        this.numCemeteriesText.text = publicPlayerInfo.Cemetery.Length.ToString();
        this.numExcludedsText.text = publicPlayerInfo.Excluded.Length.ToString();
        this.numHandsText.text = publicPlayerInfo.HandsCount.ToString();
    }

    public void SetActiveTurn(bool value)
    {
        this.outlineImage.gameObject.SetActive(value);
    }

    /// <summary>
    /// 敵プレイヤーアイコンのクリックイベント
    /// </summary>
    /// <param name="eventData"></param>
    async void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (this.IsPicked)
        {
            this.unPick(this);
        }
        else if (this.IsPickCandidate)
        {
            this.pick(this);
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
