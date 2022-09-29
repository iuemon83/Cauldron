using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ListGameHistoriesRequest
    {
        public GameId[] GameIdList { get; }

        public ListGameHistoriesRequest(GameId[] GameIdList = default)
        {
            this.GameIdList = GameIdList ?? Array.Empty<GameId>();
        }
    }
}
