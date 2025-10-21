using Dapper;
using Mairie.DAL.Configuration;
using Mairie.Domain.Entities;
using Mairie.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.DAL.Services
{
    public class AuditService : BaseRepository, IAuditService
    {
        public AuditService(DatabaseConfiguration dbConfig) : base(dbConfig)
        {
        }

        public async Task<int> CreateAuditLog(AuditLog auditLog)
        {
            const string sql = @"
            INSERT INTO AuditLog 
                (WindowsId, Action, DateAction, Resultat )
            VALUES 
                (@WindowsId, @Action, @DateAction, @Resultat);
            
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = _dbConfig.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                auditLog.WindowsId,
                auditLog.Action,
                auditLog.DateAction,
                auditLog.Resultat
            });
        }
    }
}
