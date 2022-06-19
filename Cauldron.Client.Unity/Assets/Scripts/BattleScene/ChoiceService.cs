using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceService
{
    public static Color LimitSelectedColor = Color.red;
    public static Color NoLimitSelectedColor = Color.black;

    private readonly List<PlayerId> pickedPlayerIdList = new List<PlayerId>();
    private readonly List<CardId> pickedCardIdList = new List<CardId>();
    private readonly List<CardDefId> pickedCardDefIdList = new List<CardDefId>();

    public AskMessage AskMessage { get; private set; }

    private bool CanPick => this.CurrentNumPicks + 1 <= this.AskMessage.NumPicks;

    public bool IsChoiceMode => this.AskMessage != null;

    public bool IsLimit => this.CurrentNumPicks >= this.LimitNumPicks;
    public int LimitNumPicks => this.AskMessage?.NumPicks ?? 0;
    public int CurrentNumPicks => this.Answer.Count();

    public ChoiceAnswer Answer => new ChoiceAnswer(
        this.pickedPlayerIdList.ToArray(),
        this.pickedCardIdList.ToArray(),
        this.pickedCardDefIdList.ToArray()
        );

    public bool ValidChoiceAnwser(ChoiceAnswer answer)
    {
        if (this.AskMessage.NumPicks < answer.Count())
        {
            return false;
        }

        return true;
    }

    public void Pick(PlayerController playerController)
    {
        if (!this.CanPick)
        {
            return;
        }

        this.pickedPlayerIdList.Add(playerController.PlayerId);
        playerController.ResetAllIcon();
        playerController.VisiblePickedIcon(true);
    }

    public void UnPick(PlayerController playerController)
    {
        this.pickedPlayerIdList.Remove(playerController.PlayerId);
        playerController.ResetAllIcon();
        playerController.VisiblePickCandidateIcon(true);
    }

    public void Pick(CardController cardController)
    {
        if (!this.CanPick)
        {
            return;
        }

        this.pickedCardIdList.Add(cardController.CardId);
        cardController.ResetAllIcon();
        cardController.VisiblePickedIcon(true);
    }

    public void UnPick(CardController cardController)
    {
        this.pickedCardIdList.Remove(cardController.CardId);
        cardController.ResetAllIcon();
        cardController.VisiblePickCandidateIcon(true);
    }

    public void Init(AskMessage askMessage)
    {
        this.Reset();
        this.AskMessage = askMessage;
    }

    public void Reset()
    {
        this.AskMessage = null;
        this.pickedCardDefIdList.Clear();
        this.pickedCardIdList.Clear();
        this.pickedPlayerIdList.Clear();
    }
}
