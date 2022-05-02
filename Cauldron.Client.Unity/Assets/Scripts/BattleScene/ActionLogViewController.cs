using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionLogViewController : MonoBehaviour
{
    private static readonly float ShouldScrollToBottomThreshold = 0.05f;

    [SerializeField]
    private ScrollRect ActionLogList = default;
    [SerializeField]
    private ActionLogView_ListNodeController listNodePrefab = default;
    [SerializeField]
    private CardDetailController cardDetailController = default;

    private readonly List<ActionLogView_ListNodeController> nodeList = new List<ActionLogView_ListNodeController>();

    private Transform actionLogListContent;

    private CardEffectId previousEffectId = default;

    public void Start()
    {
        this.actionLogListContent = this.ActionLogList.transform.Find("Viewport").transform.Find("Content");
    }

    public async UniTask AddLog(ActionLog actionLog, Card effectOwnerCard = default, CardEffectId? effectId = default)
    {
        if (effectOwnerCard?.Id != null
            && effectId != default
            && effectId != previousEffectId)
        {
            this.previousEffectId = effectId.Value;
            await this.AddListNode(new ActionLog("発動", effectOwnerCard), false);
        }

        await this.AddListNode(actionLog, effectOwnerCard != default);
    }

    public void OnCloseButtonClick()
    {
        Destroy(this.gameObject);
    }

    private async UniTask AddListNode(ActionLog actionLog, bool isChild)
    {
        Debug.Log($"{actionLog.Message}, {actionLog.Card?.Name}, {actionLog.PlayerInfo?.Name}");

        if (this.actionLogListContent == null)
        {
            return;
        }

        var nodeController = Instantiate(this.listNodePrefab);
        nodeController.Init(actionLog,
            this.cardDetailController.SetCard,
            isChild
            );

        nodeController.transform.SetParent(this.actionLogListContent.transform, false);

        this.nodeList.Add(nodeController);

        Debug.Log("actionlogのスクロール=" + this.ActionLogList.verticalNormalizedPosition);

        var shouldScrollToBottom = this.ActionLogList.verticalNormalizedPosition < ShouldScrollToBottomThreshold;

        await UniTask.DelayFrame(1);

        if (shouldScrollToBottom)
        {
            this.ActionLogList.verticalNormalizedPosition = 0f;
        }
    }
}
