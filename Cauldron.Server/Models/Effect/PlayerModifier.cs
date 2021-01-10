using Cauldron.Server.Models.Effect.Value;

namespace Cauldron.Server.Models.Effect
{
    public record PlayerModifier(
        NumValueModifier MaxHp = null,
        NumValueModifier Hp = null,
        NumValueModifier MaxMp = null,
        NumValueModifier Mp = null
        )
    {

    }
}
