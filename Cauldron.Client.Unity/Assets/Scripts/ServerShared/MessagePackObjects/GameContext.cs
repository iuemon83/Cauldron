using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class GameContext
    {
        public PlayerId WinnerPlayerId { get; }

        public PlayerId ActivePlayerId { get; }

        public Card[] TemporaryCards { get; }

        public PublicPlayerInfo Opponent { get; }

        public PrivatePlayerInfo You { get; }

        public RuleBook RuleBook { get; }

        public bool GameOver { get; }

        public GameContext(
            PlayerId WinnerPlayerId,
            PlayerId ActivePlayerId,
            Card[] TemporaryCards,
            PublicPlayerInfo Opponent,
            PrivatePlayerInfo You,
            RuleBook RuleBook,
            bool GameOver = false
            )
        {
            this.WinnerPlayerId = WinnerPlayerId;
            this.ActivePlayerId = ActivePlayerId;
            this.TemporaryCards = TemporaryCards;
            this.Opponent = Opponent;
            this.You = You;
            this.RuleBook = RuleBook;
            this.GameOver = GameOver;
        }
    }
}
