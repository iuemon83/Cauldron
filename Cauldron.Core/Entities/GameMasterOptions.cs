using Cauldron.Shared.MessagePackObjects;
using Microsoft.Extensions.Logging;

namespace Cauldron.Core.Entities
{
    public record GameMasterOptions(
        RuleBook RuleBook,
        CardRepository CardFactory,
        ILogger Logger,
        GameEventListener EventListener
        )
    {
    }
}
