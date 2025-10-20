using Dapper;
using Mairie.DAL.Configuration;
using Mairie.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.DAL.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly DatabaseConfiguration _dbConfig;

        public UserRoleRepository(DatabaseConfiguration dbConfig)
        {
            _dbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig));
        }

        public async Task<IEnumerable<string>> GetRolesByWindowsIdAsync(string windowsId)
        {
            const string sql = @"SELECT Role FROM UserRoles WHERE WindowsId = @windowsId";
            using var connection = _dbConfig.CreateConnection();
            return await connection.QueryAsync<string>(sql, new { windowsId });
        }
    }
}
