using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class CloseGameRequest
    {
        public GameId GameId { get; }

        public CloseGameRequest(GameId gameId)
        {
            this.GameId = gameId;
        }
    }
}
