using Cauldron.Shared.MessagePackObjects;
using MessagePack;

namespace Assets.Scripts.ServerShared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class GameOutline
    {
        public GameId GameId { get; }
        public string OwnerName { get; }

        public GameOutline(GameId gameId, string ownerName)
        {
            this.GameId = gameId;
            this.OwnerName = ownerName;
        }
    }
}
