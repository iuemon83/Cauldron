using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ListGameHistoriesRequest
    {
        public string ClientId { get; }

        public GameId[] GameIdList { get; }

        public bool OnlyMyLogs { get; }

        public ListGameHistoriesRequest(
            string clientId,
            bool OnlyMyLogs,
            GameId[] GameIdList = default
            )
        {
            this.ClientId = clientId;
            this.OnlyMyLogs = OnlyMyLogs;
            this.GameIdList = GameIdList ?? Array.Empty<GameId>();
        }
    }
}
