using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities
{
    public record PlayerDef(PlayerId Id, string Name, IReadOnlyList<CardDefId> DeckIdList)
    {
    }
}
