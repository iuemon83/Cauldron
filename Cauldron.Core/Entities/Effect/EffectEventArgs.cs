using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record EffectEventArgs(
        GameEvent GameEvent,
        GameMaster GameMaster,
        Player SourcePlayer = null,
        Card SourceCard = null,
        DamageContext DamageContext = null,
        MoveCardContext MoveCardContext = null
        );
}
