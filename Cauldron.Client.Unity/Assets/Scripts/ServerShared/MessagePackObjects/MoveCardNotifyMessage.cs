using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class MoveCardNotifyMessage
    {
        public CardId CardId { get; set; }
        public Zone ToZone { get; set; }
    }
}
