using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class JoinRoomRequest
    {
        public GameId GameId { get; }
        public string PlayerName { get; }
        public CardDefId[] DeckCardIdList { get; }
        public string ClientId { get; }

        public JoinRoomRequest(
            GameId gameId,
            string PlayerName,
            CardDefId[] DeckCardIdList,
            string ClientId
            )
        {
            this.GameId = gameId;
            this.PlayerName = PlayerName;
            this.DeckCardIdList = DeckCardIdList;
            this.ClientId = ClientId;
        }
    }
}
