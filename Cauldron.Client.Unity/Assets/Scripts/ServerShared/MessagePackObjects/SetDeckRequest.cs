using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class SetDeckRequest
    {
        public GameId GameId { get; }
        public PlayerId PlayerId { get; }
        public CardId[] DeckCardIdList { get; }

        public SetDeckRequest(GameId gameId, PlayerId PlayerId, CardId[] DeckCardIdList)
        {
            this.GameId = gameId;
            this.PlayerId = PlayerId;
            this.DeckCardIdList = DeckCardIdList;
        }
    }
}
