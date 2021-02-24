using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class PlayFromHandRequest
    {
        public GameId GameId { get; }
        public PlayerId PlayerId { get; }
        public CardId HandCardId { get; }

        public PlayFromHandRequest(GameId GameId, PlayerId PlayerId, CardId HandCardId)
        {
            this.GameId = GameId;
            this.PlayerId = PlayerId;
            this.HandCardId = HandCardId;
        }
    }
}
