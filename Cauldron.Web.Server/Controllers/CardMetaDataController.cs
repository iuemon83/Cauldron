using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Cauldron.Web.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardMetaDataController : ControllerBase
    {
        private readonly ILogger<CardMetaDataController> _logger;

        public CardMetaDataController(ILogger<CardMetaDataController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public CardMetaData Get()
        {
            this._logger.LogInformation(nameof(this.Get));

            return new CardMetaData();
        }
    }
}
