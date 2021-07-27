using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class AddCardNotifyMessage
    {
        public CardId CardId { get; }
        public Zone ToZone { get; }

        public AddCardNotifyMessage(CardId CardId, Zone ToZone)
        {
            this.CardId = CardId;
            this.ToZone = ToZone;
        }
    }
}
