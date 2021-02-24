using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyCardNotifyMessage
    {
        public CardId CardId { get; set; }
    }
}
