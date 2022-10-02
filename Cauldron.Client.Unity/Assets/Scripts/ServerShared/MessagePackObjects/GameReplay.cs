using Cauldron.Shared.MessagePackObjects;
using MessagePack;
using System;

namespace Assets.Scripts.ServerShared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class GameReplay
    {
        public GameId GameId { get; }
        public GameReplayPlayer[] Players { get; }
        public int ActionLogId { get; }
        public DateTime DateTime { get; }
        public CardDef[] CardPool { get; }

        public GameReplay(
            GameId GameId,
            GameReplayPlayer[] Players,
            int ActionLogId,
            DateTime DateTime,
            CardDef[] CardPool
            )
        {
            this.GameId = GameId;
            this.Players = Players;
            this.ActionLogId = ActionLogId;
            this.DateTime = DateTime;
            this.CardPool = CardPool;
        }
    }

    [MessagePackObject(true)]
    public class GameReplayPlayer
    {
        public PlayerId Id { get; }
        public string Name { get; }

        public GameReplayPlayer(
            PlayerId Id,
            string Name
            )
        {
            this.Id = Id;
            this.Name = Name;
        }
    }
}
