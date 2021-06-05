using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceDialogController : MonoBehaviour
{
    [SerializeField]
    private GameObject CandidatesView;
    [SerializeField]
    private ChoiceDialog_ListNodeController listNodePrefab;
    [SerializeField]
    private CardDefDetailController cardDefDetailController;
    [SerializeField]
    private Button okButton;
    [SerializeField]
    private TextMeshProUGUI countText;

    private Transform cardPoolListContent;

    private List<ChoiceDialog_ListNodeController> nodeList = new List<ChoiceDialog_ListNodeController>();

    private Action<ChoiceAnswer> okAction;
    private int choiceCount => this.nodeList.Sum(c => c.CurrentCount);
    private AskMessage askMessage;

    // Start is called before the first frame update
    void Start()
    {
        this.cardPoolListContent = this.CandidatesView.transform.Find("Viewport").transform.Find("Content");

        foreach (var group in askMessage.ChoiceCandidates.CardDefList.ToLookup(c => c.FullName))
        {
            var cardDef = group.First();
            var limit = group.Count();

            this.AddToCardPool(cardDef, limit);
        }
    }

    public void Init(AskMessage askMessage, Action<ChoiceAnswer> okAction)
    {
        this.askMessage = askMessage;
        this.okAction = okAction;
    }

    public void OnOkButtonClick()
    {
        var selectedList = this.nodeList
            .SelectMany(x => Enumerable.Repeat(x.Source.Id, x.CurrentCount))
            .ToArray();
        var answer = new ChoiceAnswer(default, default, selectedList);

        this.okAction?.Invoke(answer);
    }

    private void Choice(ChoiceDialog_ListNodeController controller)
    {
        if (this.choiceCount == this.askMessage.NumPicks)
        {
            return;
        }

        controller.AddOne();
        this.UpdateDeckTotalCountText();
    }

    private void UnChoice(ChoiceDialog_ListNodeController controller)
    {
        if (this.choiceCount == 0)
        {
            return;
        }

        controller.RemoveOne();
        this.UpdateDeckTotalCountText();
    }

    private void AddToCardPool(CardDef cardDef, int limit)
    {
        var node = Instantiate(this.listNodePrefab, this.cardPoolListContent.transform);
        var controller = node.GetComponent<ChoiceDialog_ListNodeController>();
        controller.Init(cardDef, limit,
            () => this.Choice(controller),
            () => this.UnChoice(controller),
            this.cardDefDetailController.SetCard
            );

        this.nodeList.Add(controller);
    }

    private void UpdateDeckTotalCountText()
    {
        this.countText.text = $"{this.choiceCount} / {this.askMessage.NumPicks}";

        this.countText.color = this.choiceCount == this.askMessage.NumPicks
            ? Color.red
            : Color.white;
    }
}
