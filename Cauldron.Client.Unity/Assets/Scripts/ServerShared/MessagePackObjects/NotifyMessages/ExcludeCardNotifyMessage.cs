using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ExcludeCardNotifyMessage
    {
        public Card Card { get; }
        public Zone FromZone { get; }

        public ExcludeCardNotifyMessage(Card Card, Zone FromZone)
        {
            this.Card = Card;
            this.FromZone = FromZone;
        }
    }
}
