using Cauldron.Shared.MessagePackObjects;
using MessagePack;
using System;

namespace Assets.Scripts.ServerShared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class GameReplay
    {
        public GameId GameId { get; }
        public PlayerId[] PlayerIdList { get; }
        public int ActionLogId { get; }
        public DateTime DateTime { get; }
        public CardDef[] CardPool { get; }

        public GameReplay(
            GameId GameId,
            PlayerId[] PlayerIdList,
            int ActionLogId,
            DateTime DateTime,
            CardDef[] CardPool
            )
        {
            this.GameId = GameId;
            this.PlayerIdList = PlayerIdList;
            this.ActionLogId = ActionLogId;
            this.DateTime = DateTime;
            this.CardPool = CardPool;
        }
    }
}
