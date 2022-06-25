using Cauldron.Shared.MessagePackObjects;
using MessagePack;

namespace Assets.Scripts.ServerShared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class RoomOutline
    {
        public GameId GameId { get; }
        public string OwnerName { get; }
        public string Message { get; }

        public RoomOutline(GameId gameId, string ownerName, string message)
        {
            this.GameId = gameId;
            this.OwnerName = ownerName;
            this.Message = message;
        }
    }
}
