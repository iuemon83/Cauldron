namespace Cauldron.Grpc.Models
{
    public partial class RuleBook
    {
        public RuleBook(Server.Models.RuleBook source)
        {
            this.InitialMp = source.InitialMp;
            this.InitialNumHands = source.InitialNumHands;
            this.InitialPlayerHp = source.InitialPlayerHp;
            this.MaxMp = source.MaxLimitMp;
            this.MaxNumDeckCards = source.MaxNumDeckCards;
            this.MaxNumFieldCars = source.MaxNumFieldCars;
            this.MaxNumHands = source.MaxNumHands;
            this.MaxPlayerHp = source.MaxPlayerHp;
            this.MinMp = source.MinMp;
            this.MinNumDeckCards = source.MinNumDeckCards;
            this.MinPlayerHp = source.MinPlayerHp;
            this.MpByStep = source.MpByStep;
        }
    }
}
