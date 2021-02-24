using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class AddCardNotifyMessage
    {
        public CardId CardId { get; set; }
        public Zone ToZone { get; set; }
    }
}
