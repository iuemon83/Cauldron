namespace Cauldron.Server.Models.Effect
{
    public record DamageContext(
        Card DamageSourceCard,
        int Value,
        Card GuardCard = null,
        Player GuardPlayer = null
        );
}