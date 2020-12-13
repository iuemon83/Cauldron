namespace Cauldron.Server.Models.Effect
{
    public record EffectEventArgs(
        GameEvent GameEvent,
        GameMaster GameMaster,
        Player SourcePlayer = null,
        Card SourceCard = null,
        BattleContext BattleContext = null,
        DamageContext DamageContext = null,
        MoveCardContext MoveCardContext = null
        );
}
