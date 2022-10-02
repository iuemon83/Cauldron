using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;

namespace Cauldron.Server
{
    public record GameLog(
        GameId GameId,
        IReadOnlyList<CardDef> CardDefsJson,
        PlayerId WinnerPlayerId = default
        );
}
