namespace Cauldron.Server.Models.Effect
{
    public class BattleContext
    {
        public Card AttackCard { get; set; }
        public Card GuardCard { get; set; }

        public Player GuardPlayer { get; set; }

        public int Value { get; set; }
    }
}