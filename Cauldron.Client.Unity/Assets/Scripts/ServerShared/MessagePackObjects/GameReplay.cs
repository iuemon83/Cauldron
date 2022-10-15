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

        public GameReplay(
            GameId GameId,
            GameReplayPlayer[] Players,
            int ActionLogId,
            DateTime DateTime
            )
        {
            this.GameId = GameId;
            this.Players = Players;
            this.ActionLogId = ActionLogId;
            this.DateTime = DateTime;
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
