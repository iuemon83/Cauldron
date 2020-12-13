namespace Cauldron.Server.Models
{
    public record RuleBook(
        int InitialPlayerHp = 20,
        int MaxPlayerHp = 20,
        int MinPlayerHp = 0,
        int MaxNumDeckCards = 30,
        int MinNumDeckCards = 30,
        int InitialNumHands = 5,
        int MaxNumHands = 9,
        int InitialMp = 0,
        int MaxLimitMp = 10,
        int MinMp = 0,
        int MpByStep = 1,
        int MaxNumFieldCars = 5
        )
    {
        public RuleBook(Cauldron.Grpc.Models.RuleBook other)
            : this(
                 other.InitialPlayerHp,
                 other.MaxPlayerHp,
                 other.MinPlayerHp,
                 other.MaxNumDeckCards,
                 other.MinNumDeckCards,
                 other.InitialNumHands,
                 other.MaxNumHands,
                 other.InitialMp,
                 other.MaxMp,
                 other.MinMp,
                 other.MpByStep,
                 other.MaxNumFieldCars
                 )
        { }
    }
}
