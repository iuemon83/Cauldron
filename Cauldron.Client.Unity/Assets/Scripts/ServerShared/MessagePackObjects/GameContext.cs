﻿using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class GameContext
    {
        public PlayerId WinnerPlayerId { get; }

        public PublicPlayerInfo Opponent { get; }

        public PrivatePlayerInfo You { get; }

        public RuleBook RuleBook { get; }

        public bool GameOver { get; }

        public GameContext(
            PlayerId WinnerPlayerId,
            PublicPlayerInfo Opponent,
            PrivatePlayerInfo You,
             RuleBook RuleBook,
            bool GameOver = false
            )
        {
            this.WinnerPlayerId = WinnerPlayerId;
            this.Opponent = Opponent;
            this.You = You;
            this.RuleBook = RuleBook;
            this.GameOver = GameOver;
        }
    }
}
