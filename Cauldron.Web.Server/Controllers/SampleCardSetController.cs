using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cauldron.Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleCardSetController : ControllerBase
    {
        private readonly ILogger<SampleCardSetController> _logger;

        public SampleCardSetController(ILogger<SampleCardSetController> logger)
        {
            _logger = logger;
        }

        public string Get()
        {
            this._logger.LogInformation(nameof(this.Get));

            return System.IO.File.ReadAllText(@"Resources/sample_cardset.json");
        }
    }
}
