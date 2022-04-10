using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Reflection;

namespace Cauldron.Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SampleCardSetController : ControllerBase
    {
        private readonly ILogger<SampleCardSetController> _logger;
        private readonly IWebHostEnvironment env;

        public SampleCardSetController(ILogger<SampleCardSetController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            this.env = env;
        }

        [HttpGet("{id}")]
        public string Get(int id)
        {
            this._logger.LogInformation(nameof(this.Get));

            var filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @$"Resources/sample_cardset{id}.json");
            return System.IO.File.ReadAllText(filePath);
        }
    }
}
