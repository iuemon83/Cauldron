using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EndTurnRequest
    {
        public GameId GameId { get; }
        public PlayerId PlayerId { get; }

        public EndTurnRequest(GameId GameId, PlayerId PlayerId)
        {
            this.GameId = GameId;
            this.PlayerId = PlayerId;
        }
    }
}
