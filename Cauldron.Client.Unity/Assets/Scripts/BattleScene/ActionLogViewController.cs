using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionLogViewController : MonoBehaviour
{
    private static readonly float ShouldScrollToBottomThreshold = 0f;

    [SerializeField]
    private ScrollRect ActionLogList = default;
    [SerializeField]
    private ActionLogView_ListNodeController listNodePrefab = default;
    [SerializeField]
    private CardDetailController cardDetailController = default;
    [SerializeField]
    private Transform HiddenContainer = default;
    [SerializeField]
    private Transform DisplayContainer = default;

    private readonly List<ActionLogView_ListNodeController> nodeList = new List<ActionLogView_ListNodeController>();

    private Transform actionLogListContent;
    private bool isDisplay = false;

    public void Start()
    {
        this.actionLogListContent = this.ActionLogList.transform.Find("Viewport").transform.Find("Content");
    }

    public async UniTask AddLog(ActionLog actionLog)
    {
        await this.AddListNode(actionLog);
    }

    public void ToggleDisplay()
    {
        if (this.isDisplay)
        {
            this.transform.SetParent(this.HiddenContainer.transform, false);
        }
        else
        {
            this.transform.SetParent(this.DisplayContainer.transform, false);
        }

        this.isDisplay = !this.isDisplay;
    }

    public void OnCloseButtonClick()
    {
        Destroy(this.gameObject);
    }

    private async UniTask AddListNode(ActionLog actionLog)
    {
        Debug.Log($"{actionLog.Message}, {actionLog.Card?.Name}, {actionLog.PlayerInfo?.Name}");

        if (this.actionLogListContent == null)
        {
            return;
        }

        var controller = Instantiate(this.listNodePrefab, this.actionLogListContent.transform);
        controller.Init(actionLog,
            this.cardDetailController.SetCard
            );

        this.nodeList.Add(controller);

        var shouldScrollToBottom = this.ActionLogList.verticalNormalizedPosition < ShouldScrollToBottomThreshold;

        await UniTask.DelayFrame(1);

        if (shouldScrollToBottom)
        {
            this.ActionLogList.verticalNormalizedPosition = 0f;
        }
    }
}
