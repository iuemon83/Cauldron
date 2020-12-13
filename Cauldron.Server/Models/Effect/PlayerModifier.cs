namespace Cauldron.Server.Models.Effect
{
    public record PlayerModifier(
        ValueModifier MaxHp = null,
        ValueModifier Hp = null,
        ValueModifier MaxMp = null,
        ValueModifier Mp = null
        )
    {

    }
}
