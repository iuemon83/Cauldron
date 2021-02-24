using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class RuleBook
    {
        public int InitialPlayerHp { get; }
        public int MaxPlayerHp { get; }
        public int MinPlayerHp { get; }
        public int MaxNumDeckCards { get; }
        public int MinNumDeckCards { get; }
        public int InitialNumHands { get; }
        public int MaxNumHands { get; }
        public int InitialMp { get; }
        public int MaxLimitMp { get; }
        public int MinMp { get; }
        public int MpByStep { get; }
        public int MaxNumFieldCars { get; }
        public int DefaultNumTurnsToCanAttack { get; }
        public int DefaultNumAttacksLimitInTurn { get; }

        public RuleBook(
            int InitialPlayerHp,
            int MaxPlayerHp,
            int MinPlayerHp,
            int MaxNumDeckCards,
            int MinNumDeckCards,
            int InitialNumHands,
            int MaxNumHands,
            int InitialMp,
            int MaxLimitMp,
            int MinMp,
            int MpByStep,
            int MaxNumFieldCars,
            int DefaultNumTurnsToCanAttack,
            int DefaultNumAttacksLimitInTurn
            )
        {
            this.InitialPlayerHp = InitialPlayerHp;
            this.MaxPlayerHp = MaxPlayerHp;
            this.MinPlayerHp = MinPlayerHp;
            this.MaxNumDeckCards = MaxNumDeckCards;
            this.MinNumDeckCards = MinNumDeckCards;
            this.InitialNumHands = InitialNumHands;
            this.MaxNumHands = MaxNumHands;
            this.InitialMp = InitialMp;
            this.MaxLimitMp = MaxLimitMp;
            this.MinMp = MinMp;
            this.MpByStep = MpByStep;
            this.MaxNumFieldCars = MaxNumFieldCars;
            this.DefaultNumTurnsToCanAttack = DefaultNumTurnsToCanAttack;
            this.DefaultNumAttacksLimitInTurn = DefaultNumAttacksLimitInTurn;
        }
    }
}
