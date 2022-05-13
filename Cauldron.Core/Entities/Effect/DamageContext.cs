using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record DamageContext(
        DamageNotifyMessage.ReasonValue Reason,
        Card? DamageSourceCard,
        int Value,
        Card? GuardCard = null,
        Player? GuardPlayer = null
        );
}
