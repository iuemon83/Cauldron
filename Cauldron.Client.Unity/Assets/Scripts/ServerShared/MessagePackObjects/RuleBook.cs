using MessagePack;
using System.IO;

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
        public int LimitMpToIncrease { get; }
        public int MaxNumFieldCards { get; }
        public int DefaultNumTurnsToCanAttack { get; }
        public int DefaultNumAttacksLimitInTurn { get; }
        public int DefaultLimitNumCardsInDeck { get; }

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
            int LimitMpToIncrease,
            int MaxNumFieldCards,
            int DefaultNumTurnsToCanAttack,
            int DefaultNumAttacksLimitInTurn,
            int DefaultLimitNumCardsInDeck
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
            this.LimitMpToIncrease = LimitMpToIncrease;
            this.MaxNumFieldCards = MaxNumFieldCards;
            this.DefaultNumTurnsToCanAttack = DefaultNumTurnsToCanAttack;
            this.DefaultNumAttacksLimitInTurn = DefaultNumAttacksLimitInTurn;
            this.DefaultLimitNumCardsInDeck = DefaultLimitNumCardsInDeck;
        }
    }
}
