using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EnterGameRequest
    {
        public GameId GameId { get; }
        public string PlayerName { get; }
        public CardDefId[] DeckCardIdList { get; }

        public EnterGameRequest(GameId gameId, string PlayerName, CardDefId[] DeckCardIdList)
        {
            this.GameId = gameId;
            this.PlayerName = PlayerName;
            this.DeckCardIdList = DeckCardIdList;
        }
    }
}
