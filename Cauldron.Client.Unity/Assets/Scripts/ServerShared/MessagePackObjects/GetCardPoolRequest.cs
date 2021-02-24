using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class GetCardPoolRequest
    {
        public GameId GameId { get; }

        public GetCardPoolRequest(GameId gameId)
        {
            this.GameId = gameId;
        }
    }
}
