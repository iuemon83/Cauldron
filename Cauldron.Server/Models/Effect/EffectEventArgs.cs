namespace Cauldron.Server.Models.Effect
{
    public class EffectEventArgs
    {
        public GameEvent EffectType { get; set; }
        public GameMaster GameMaster { get; set; }
        public Player SourcePlayer { get; set; }
        public Card SourceCard { get; set; }
        public BattleContext BattleContext { get; set; }
        public DamageContext DamageContext { get; set; }
    }
}
