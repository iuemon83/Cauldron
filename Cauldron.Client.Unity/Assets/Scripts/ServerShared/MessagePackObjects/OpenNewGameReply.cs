using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class OpenNewGameReply
    {
        public GameId GameId { get; }

        public OpenNewGameReply(GameId gameId)
        {
            this.GameId = gameId;
        }
    }
}
