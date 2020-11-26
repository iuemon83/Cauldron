using System;

namespace Cauldron.Core
{
    public class GameEnvironment
    {
        public bool GameOver { get; set; } = false;
        public Guid WinnerPlayerId { get; set; }

        public PublicPlayerInfo Opponent { get; set; }

        public PrivatePlayerInfo You { get; set; }
    }
}
