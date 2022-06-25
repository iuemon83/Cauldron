using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class OpenNewRoomReply
    {
        public enum StatusCodeValue
        {
            Ok,
            InvalidDeck
        }

        public StatusCodeValue StatusCode { get; }

        public GameId GameId { get; }

        public PlayerId PlayerId { get; }

        public OpenNewRoomReply(StatusCodeValue statusCode, GameId gameId, PlayerId playerId)
        {
            this.StatusCode = statusCode;
            this.GameId = gameId;
            this.PlayerId = playerId;
        }
    }
}
