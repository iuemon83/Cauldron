using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities.Effect
{
    public record MoveCardContext(Zone From, Zone To, InsertCardPosition InsertCardPosition = null);
}