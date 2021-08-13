using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ExcludeCardNotifyMessage
    {
        public CardId CardId { get; }

        public ExcludeCardNotifyMessage(CardId CardId)
        {
            this.CardId = CardId;
        }
    }
}
