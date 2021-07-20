using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class MoveCardNotifyMessage
    {
        public CardId CardId { get; }
        public Zone ToZone { get; }
        public int Index { get; }

        public MoveCardNotifyMessage(CardId CardId, Zone ToZone, int Index)
        {
            this.CardId = CardId;
            this.ToZone = ToZone;
            this.Index = Index;
        }
    }
}
