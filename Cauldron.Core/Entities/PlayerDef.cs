using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;

namespace Cauldron.Core.Entities
{
    public record PlayerDef(PlayerId Id, string Name, IReadOnlyList<CardDefId> DeckIdList)
    {
        public bool Ready { get; set; } = false;
    }
}
