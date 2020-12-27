namespace Cauldron.Server.Models
{
    public class GameContext
    {
        public bool GameOver { get; set; } = false;
        public PlayerId WinnerPlayerId { get; set; }

        public PublicPlayerInfo Opponent { get; set; }

        public PrivatePlayerInfo You { get; set; }

        public RuleBook RuleBook { get; set; }
    }
}
