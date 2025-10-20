using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mairie.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticController : ControllerBase
    {
        [HttpGet("/health")]
        public IActionResult Get()
        {
            return Ok("API is running.");
        }
    }
}
