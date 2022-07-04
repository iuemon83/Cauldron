using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class JoinRoomReply
    {
        public enum StatusCodeValue
        {
            Ok,
            RoomIsFull,
            InvalidDeck
        }

        public PlayerId PlayerId { get; }

        public StatusCodeValue StatusCode { get; }

        public JoinRoomReply(PlayerId PlayerId, StatusCodeValue StatusCode)
        {
            this.PlayerId = PlayerId;
            this.StatusCode = StatusCode;
        }
    }
}
