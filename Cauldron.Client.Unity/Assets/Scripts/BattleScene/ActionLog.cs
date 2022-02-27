using Cauldron.Shared.MessagePackObjects;

public class ActionLog
{
    public readonly string Message;
    public readonly Card Card;
    public readonly PublicPlayerInfo PlayerInfo;

    public ActionLog(string message, Card card)
    {
        this.Message = message;
        this.Card = card;
    }
    public ActionLog(string message, PublicPlayerInfo playerInfo)
    {
        this.Message = message;
        this.PlayerInfo = playerInfo;
    }
}
