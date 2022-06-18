using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;

public class ChoiceService
{

    private readonly List<PlayerId> pickedPlayerIdList = new List<PlayerId>();
    private readonly List<CardId> pickedCardIdList = new List<CardId>();
    private readonly List<CardDefId> pickedCardDefIdList = new List<CardDefId>();

    public AskMessage AskMessage { get; private set; }

    private bool CanPick => this.Answer.Count() + 1 <= this.AskMessage.NumPicks;

    public bool IsChoiceMode => this.AskMessage != null;

    public bool IsLimit => this.Answer.Count() >= this.LimitNumPicks;
    public int LimitNumPicks => this.AskMessage?.NumPicks ?? 0;

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
        //this.NumPicks += 1;
    }

    public void UnPick(PlayerController playerController)
    {
        this.pickedPlayerIdList.Remove(playerController.PlayerId);
        playerController.ResetAllIcon();
        playerController.VisiblePickCandidateIcon(true);
        //this.NumPicks -= 1;
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
        //this.NumPicks += 1;
    }

    public void UnPick(CardController cardController)
    {
        this.pickedCardIdList.Remove(cardController.CardId);
        cardController.ResetAllIcon();
        cardController.VisiblePickCandidateIcon(true);
        //this.NumPicks -= 1;
    }

    public void Init(AskMessage askMessage)
    {
        this.Reset();
        this.AskMessage = askMessage;
    }

    public void Reset()
    {
        // リセット
        //this.NumPicks = 0;
        //this.NumPicksLimit = 0;

        this.AskMessage = null;
        this.pickedCardDefIdList.Clear();
        this.pickedCardIdList.Clear();
        this.pickedPlayerIdList.Clear();
    }
}
