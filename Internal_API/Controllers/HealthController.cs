using Microsoft.AspNetCore.Mvc;

namespace Internal_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase    {
        

        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "health")]
        public string Get()
        {
            return "healthy";
        }
    }
}
