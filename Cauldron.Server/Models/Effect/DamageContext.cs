namespace Cauldron.Server.Models.Effect
{
    public class DamageContext
    {
        public Card DamageSourceCard { get; set; }
        public Card GuardCard { get; set; }
        public Player GuardPlayer { get; set; }

        public int Value { get; set; }
    }
}