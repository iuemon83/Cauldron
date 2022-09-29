using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StartBattleLogDialogController : MonoBehaviour
{
    [SerializeField]
    private GameObject playerListView = default;

    [SerializeField]
    private ListPlayerNodeController listNodePrefab = default;

    private Transform playerListContent;
    private ListPlayerNodeController selectedNode;

    private Action<PlayerId> onOkButtonClickAction;
    private Action onCancelButtonClickAction;

    private void AddToPlayerListView(PlayerId playerId, string playerName)
    {
        var node = Instantiate(this.listNodePrefab, this.playerListContent.transform);
        var controller = node.GetComponent<ListPlayerNodeController>();
        controller.Set(playerId, playerName, this.SelectNode);
    }

    public void OnOkButtonClick()
    {
        this.gameObject.SetActive(false);
        this.onOkButtonClickAction?.Invoke(this.selectedNode.PlayerId);
    }

    public void OnCancelButtonClick()
    {
        this.gameObject.SetActive(false);
        this.onCancelButtonClickAction?.Invoke();
    }

    private void SelectNode(ListPlayerNodeController nodeController)
    {
        if (this.selectedNode != null)
        {
            this.selectedNode.SetDeselectedColor();
        }

        this.selectedNode = nodeController;
        this.selectedNode.SetSelectedColor();
    }

    public void Init(IReadOnlyList<(PlayerId, string)> list, Action<PlayerId> onOkButtonClickAction, Action onCancelButtonClickAction = null)
    {
        this.playerListContent = this.playerListView.transform.Find("Viewport").transform.Find("Content");

        // clear list
        foreach (Transform child in this.playerListContent.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var item in list)
        {
            this.AddToPlayerListView(item.Item1, item.Item2);
        }

        this.onOkButtonClickAction = onOkButtonClickAction;
        this.onCancelButtonClickAction = onCancelButtonClickAction;

        if (this.selectedNode != null)
        {
            this.selectedNode.SetDeselectedColor();
            this.selectedNode = null;
        }

        //this.gameObject.SetActive(true);
    }
}
