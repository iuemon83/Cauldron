namespace Cauldron.Server.Models
{
    public class CardDefJson
    {
        public string Name { get; set; }
        public string FlavorText { get; set; }
        public CardType Type { get; set; }
        public int Cost { get; set; }
        public int Power { get; set; }
        public int Toughness { get; set; }
        public CreatureAbility[] Abilities { get; set; }
        public string EffectText { get; set; }
    }
}
