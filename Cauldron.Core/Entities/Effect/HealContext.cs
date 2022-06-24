using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record HealContext(
        Card? HealSourceCard,
        int Value,
        Card? TakeCard = null,
        Player? TakePlayer = null
        );
}
