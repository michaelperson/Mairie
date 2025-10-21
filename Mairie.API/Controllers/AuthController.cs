using Mairie.Domain.DTOs;
using Mairie.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Mairie.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    { 
        private readonly ILogger<AuthController> _logger;
        public AuthController(  ILogger<AuthController> logger)
        {
            _logger = logger;
        }
        [HttpGet("current-user")]
        public IActionResult Get()
        {
            if(User is null) return Unauthorized();
            if (!User?.Identity?.IsAuthenticated ?? false) return Unauthorized();
            return Ok(new UserInfoDTO()
            {
                UserName = User?.Identity?.Name??"Inconnu",
                Roles = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value.ToString())
            });
        }
    }
}
