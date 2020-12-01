namespace Cauldron.Server.Models.Effect
{
    public record EffectEventArgs(
        GameEvent EffectType,
        GameMaster GameMaster,
        Player SourcePlayer = null,
        Card SourceCard = null,
        BattleContext BattleContext = null,
        DamageContext DamageContext = null);
}
