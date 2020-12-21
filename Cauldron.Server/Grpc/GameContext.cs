namespace Cauldron.Grpc.Models
{
    public partial class GameContext
    {
        public GameContext(Server.Models.GameContext source)
        {
            this.GameOver = source.GameOver;
            this.WinnerPlayerId = source.WinnerPlayerId.ToString();
            //this.RuleBook = new RuleBook(source.ru)
            this.You = new PrivatePlayerInfo(source.You);
            this.Opponent = new PublicPlayerInfo(source.Opponent);
        }

        //public Server.Models.GameEnvironment ToServerModel()
        //{
        //    return new Server.Models.GameEnvironment()
        //    {
        //        GameOver = this.GameOver,
        //        WinnerPlayerId = Guid.Parse(this.WinnerPlayerId),
        //        You = this.You.ToServerModel(),
        //        Opponent = this.Opponent.ToServerModel()
        //    };
        //}
    }
}
