using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class StartTurnRequest
    {
        public GameId GameId { get; }
        public PlayerId PlayerId { get; }

        public StartTurnRequest(GameId GameId, PlayerId PlayerId)
        {
            this.GameId = GameId;
            this.PlayerId = PlayerId;
        }
    }
}
