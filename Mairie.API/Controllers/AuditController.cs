using Mairie.Domain.DTOs;
using Mairie.Domain.Entities;
using Mairie.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Mairie.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "Administrateur")]
    public class AuditController : ControllerBase
    {

        private readonly ILogger<AuditController> _logger;
        private readonly IAuditService _auditService;

        public AuditController(ILogger<AuditController> logger, IAuditService auditService)
        {
            _logger = logger;
            _auditService = auditService;
        }
        [HttpGet]
        public async Task<IEnumerable<AuditLogDTO>> GetAll()
        {
            return (await _auditService.GetAllAsync()).Select(a => new AuditLogDTO()
            {
               WindowsId=  a.WindowsId,
               Action= a.Action,
               DateAction= a.DateAction,
               Resultat= a.Resultat
            });
        }
    }
}
