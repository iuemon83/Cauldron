using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ReadyGameRequest
    {
        public GameId GameId { get; }
        public PlayerId PlayerId { get; }

        public ReadyGameRequest(GameId gameId, PlayerId playerId)
        {
            this.GameId = gameId;
            this.PlayerId = playerId;
        }
    }
}
