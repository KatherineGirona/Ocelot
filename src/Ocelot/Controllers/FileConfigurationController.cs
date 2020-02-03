using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocelot.Services;

namespace Ocelot.Controllers
{
    [Authorize]
    [Route("configuration")]
    public class FileConfigurationController : Controller
    {
        private readonly IGetFileConfiguration _getFileConfig;

        public FileConfigurationController(IGetFileConfiguration getFileConfig)
        {
            _getFileConfig = getFileConfig;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return new OkObjectResult(_getFileConfig.Invoke().Data);
        }
    }
}