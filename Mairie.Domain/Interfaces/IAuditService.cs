using Mairie.Domain.DTOs;
using Mairie.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.Domain.Interfaces
{
    public interface IAuditService
    {
        Task<int> CreateAuditLog(AuditLog auditLog);
        Task<IEnumerable<AuditLogDTO>> GetAllAsync();
    }
}
