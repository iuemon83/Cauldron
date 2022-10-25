using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class NextActionLogRequest
    {
        public string ClientId { get; }

        public GameId GameId { get; }

        public PlayerId PlayerId { get; }

        public int CurrentActionLogId { get; }

        public NextActionLogRequest(
            string ClientId,
            GameId GameId,
            PlayerId PlayerId,
            int CurrentActionLogId

            )
        {
            this.ClientId = ClientId;
            this.GameId = GameId;
            this.PlayerId = PlayerId;
            this.CurrentActionLogId = CurrentActionLogId;
        }
    }
}
