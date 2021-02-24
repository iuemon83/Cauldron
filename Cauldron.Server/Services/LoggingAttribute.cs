using MagicOnion.Server.Hubs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Cauldron.Server.Services
{
    public class LoggingAttribute : StreamingHubFilterAttribute
    {
        private readonly ILogger _logger;

        // the `logger` parameter will be injected at instantiating.
        public LoggingAttribute(ILogger<LoggingAttribute> logger)
        {
            _logger = logger;
        }

        public override async ValueTask Invoke(StreamingHubContext context, Func<StreamingHubContext, ValueTask> next)
        {
            _logger.LogInformation($"Begin: {context.Path}");
            await next(context);
            _logger.LogInformation($"End: {context.Path}");
        }
    }
}
