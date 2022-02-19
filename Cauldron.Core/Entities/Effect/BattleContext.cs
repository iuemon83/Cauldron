using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record BattleContext(
        Card AttackCard,
        Card? GuardCard,
        Player? GuardPlayer
        );
}