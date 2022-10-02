using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ReadyGameRequest
    {
        public GameId GameId { get; }
        public PlayerId PlayerId { get; }
        public string ClientId { get; }

        public ReadyGameRequest(
            GameId gameId,
            PlayerId playerId,
            string clientId
            )
        {
            this.GameId = gameId;
            this.PlayerId = playerId;
            this.ClientId = clientId;
        }
    }
}
