using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class JoinRoomRequest
    {
        public GameId GameId { get; }
        public string PlayerName { get; }
        public CardDefId[] DeckCardIdList { get; }

        public JoinRoomRequest(GameId gameId, string PlayerName, CardDefId[] DeckCardIdList)
        {
            this.GameId = gameId;
            this.PlayerName = PlayerName;
            this.DeckCardIdList = DeckCardIdList;
        }
    }
}
