using Cauldron.Server.Models.Effect;
using Microsoft.Extensions.Logging;
using System;

namespace Cauldron.Server.Models
{
    public record GameMasterOptions(
        RuleBook RuleBook,
        CardFactory CardFactory,
        ILogger Logger,
        Func<PlayerId, ChoiceResult, int, ChoiceResult> AskCardAction,
        Action<PlayerId, Grpc.Api.ReadyGameReply> NotifyClient)
    {
    }
}
