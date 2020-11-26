namespace Cauldron.Server.Models.Effect
{
    public class EffectEventArgs
    {
        public GameEvent EffectType { get; set; }
        public GameMaster GameMaster { get; set; }
        public Card Source { get; set; }
    }
}
