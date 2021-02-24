using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record BattleContext(
        Card AttackCard,
        int Value,
        Card GuardCard = null,
        Player GuardPlayer = null
        );
}