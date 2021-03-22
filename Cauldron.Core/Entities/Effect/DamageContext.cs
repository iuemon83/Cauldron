using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record DamageContext(
        Card DamageSourceCard,
        int Value,
        Card GuardCard = null,
        Player GuardPlayer = null,
        bool IsBattle = false
        );
}