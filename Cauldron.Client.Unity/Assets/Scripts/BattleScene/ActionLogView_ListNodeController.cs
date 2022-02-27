using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionLogView_ListNodeController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI cardNameText = default;
    [SerializeField]
    private TextMeshProUGUI actionNameText = default;
    [SerializeField]
    private Image controllerIconImage = default;
    [SerializeField]
    private Image zoneIconImage = default;
    [SerializeField]
    private Image backgroundImage = default;

    private Action<Card> showCardDetailAction;

    private ActionLog actionLog;

    public void Init(ActionLog actionLog, Action<Card> ShowDetailAction)
    {
        this.actionLog = actionLog;
        this.showCardDetailAction = ShowDetailAction;

        this.cardNameText.text = this.actionLog.Card?.Name
            ?? this.actionLog.PlayerInfo?.Name
            ?? "";

        this.actionNameText.text = actionLog.Message;

        this.InitControllerIcon();
        this.InitZoneIcon(this.actionLog.Card);
        this.InitColor();
    }

    private void InitControllerIcon()
    {
        if (this.actionLog.Card == null
            && this.actionLog.PlayerInfo == null)
        {
            this.controllerIconImage.gameObject.SetActive(false);
            return;
        }

        var playerId = this.actionLog.Card?.OwnerId
            ?? this.actionLog.PlayerInfo?.Id
            ?? default;

        var controllerIconType = playerId == BattleSceneController.Instance.YouId
            ? ControllerIconCache.IconType.You
            : ControllerIconCache.IconType.Opponent;

        var (controllerSuccess, controllerIcon) = ControllerIconCache.TryGet(controllerIconType);
        if (controllerSuccess)
        {
            this.controllerIconImage.sprite = controllerIcon;
            this.controllerIconImage.gameObject.SetActive(true);
        }
    }

    private void InitZoneIcon(Card source)
    {
        if (source == null)
        {
            this.zoneIconImage.gameObject.SetActive(false);
            return;
        }

        var (zoneSuccess, zoneIcon) = ZoneIconCache.TryGet(source.Zone.ZoneName);
        if (zoneSuccess)
        {
            this.zoneIconImage.sprite = zoneIcon;
            this.zoneIconImage.gameObject.SetActive(true);
        }
        else
        {
            this.zoneIconImage.gameObject.SetActive(false);
        }
    }

    private void InitColor()
    {
        var playerId = this.actionLog.Card?.OwnerId
            ?? this.actionLog.PlayerInfo?.Id
            ?? default;

        this.backgroundImage.color = playerId == BattleSceneController.Instance.YouId
            ? BattleSceneController.YouColor
            : BattleSceneController.OpponentColor;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            this.showCardDetailAction(this.actionLog.Card);
        }
    }
}
