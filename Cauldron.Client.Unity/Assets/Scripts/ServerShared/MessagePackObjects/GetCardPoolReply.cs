using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class GetCardPoolReply
    {
        public CardDef[] Cards { get; }

        public GetCardPoolReply(CardDef[] Cards)
        {
            this.Cards = Cards;
        }
    }
}
