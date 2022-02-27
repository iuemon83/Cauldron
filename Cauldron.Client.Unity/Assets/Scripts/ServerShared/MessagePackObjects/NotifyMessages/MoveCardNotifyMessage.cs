using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class MoveCardNotifyMessage
    {
        public Card Card { get; }
        public Zone FromZone { get; }
        public Zone ToZone { get; }
        public int Index { get; }

        public MoveCardNotifyMessage(Card Card, Zone FromZone, Zone ToZone, int Index)
        {
            this.Card = Card;
            this.FromZone = FromZone;
            this.ToZone = ToZone;
            this.Index = Index;
        }
    }
}
