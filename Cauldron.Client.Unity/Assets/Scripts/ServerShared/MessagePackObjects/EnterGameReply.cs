using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EnterGameReply
    {
        public enum StatusCodeValue
        {
            Ok,
            RoomIsFull,
            InvalidDeck
        }

        public PlayerId PlayerId { get; }

        public StatusCodeValue StatusCode { get; }

        public EnterGameReply(PlayerId PlayerId, StatusCodeValue StatusCode)
        {
            this.PlayerId = PlayerId;
            this.StatusCode = StatusCode;
        }
    }
}
