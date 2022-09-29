using Cauldron.Shared.MessagePackObjects;
using MessagePack;

namespace Assets.Scripts.ServerShared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ReplayActionLog
    {
        public int ActionLogId { get; }
        public PlayerId SendPlayerId { get; }
        public NotifyEvent NotifyEvent { get; }
        public GameContext GameContext { get; }
        public string MessageJson { get; }

        public ReplayActionLog(
            int ActionLogId,
            PlayerId SendPlayerId,
            NotifyEvent NotifyEvent,
            GameContext GameContext,
            string MessageJson
            )
        {
            this.ActionLogId = ActionLogId;
            this.SendPlayerId = SendPlayerId;
            this.NotifyEvent = NotifyEvent;
            this.GameContext = GameContext;
            this.MessageJson = MessageJson;
        }
    }
}
