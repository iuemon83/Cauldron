using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyCardNotifyMessage
    {
        public Card Card { get; }

        public ModifyCardNotifyMessage(Card Card)
        {
            this.Card = Card;
        }
    }
}
