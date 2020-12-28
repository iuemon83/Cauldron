using System.Collections.Generic;

namespace Cauldron.Server.Models
{
    public record PlayerDef(PlayerId Id, string Name, IReadOnlyList<CardDefId> DeckIdList);
}
