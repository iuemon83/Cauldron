using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PlayCardNotifyMessage
    {
        public PlayerId PlayerId { get; }
        public CardDefId CardDefId { get; }

        public PlayCardNotifyMessage(PlayerId PlayerId, CardDefId CardDefId)
        {
            this.PlayerId = PlayerId;
            this.CardDefId = CardDefId;
        }
    }
}
