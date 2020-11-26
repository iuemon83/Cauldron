namespace Cauldron.Core
{
    public class RuleBook
    {
        public int InitialPlayerHp { get; set; } = 20;
        public int MaxPlayerHp { get; set; } = 20;
        public int MinPlayerHp { get; set; } = 0;
        public int MaxNumDeckCards { get; set; } = 30;
        public int MinNumDeckCards { get; set; } = 30;
        public int InitialNumHands { get; set; } = 5;
        public int MaxNumHands { get; set; } = 9;
        public int InitialMp { get; set; } = 0;
        public int MaxMp { get; set; } = 10;
        public int MinMp { get; set; } = 0;
        public int MpByStep { get; set; } = 1;
        public int MaxNumFieldCars { get; set; } = 5;
    }
}
