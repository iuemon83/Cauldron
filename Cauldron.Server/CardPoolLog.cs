using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;

namespace Cauldron.Server
{
    public record CardPoolLog(
        GameId GameId,
        IReadOnlyList<CardDef> CardDefsJson
        );
}
