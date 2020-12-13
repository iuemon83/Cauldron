namespace Cauldron.Server.Models.Effect
{
    public record BattleContext(
        Card AttackCard,
        int Value,
        Card GuardCard = null,
        Player GuardPlayer = null
        );
}