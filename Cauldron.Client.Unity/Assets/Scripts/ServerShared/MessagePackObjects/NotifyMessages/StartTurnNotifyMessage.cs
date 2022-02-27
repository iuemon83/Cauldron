using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class StartTurnNotifyMessage
    {
        public PlayerId PlayerId { get; }

        public StartTurnNotifyMessage(PlayerId PlayerId)
        {
            this.PlayerId = PlayerId;
        }
    }
}
