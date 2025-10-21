using Dapper;
using Mairie.DAL.Configuration;
using Mairie.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.DAL.Services
{
    public class UserRoleRepository : BaseRepository, IUserRoleRepository
    {
        public UserRoleRepository(DatabaseConfiguration dbConfig) : base(dbConfig) { }

        public async Task<IEnumerable<string>> GetRolesByWindowsIdAsync(string windowsId)
        {
            const string sql = @"SELECT Role FROM UserRoles WHERE WindowsId = @windowsId";
            using var connection = _dbConfig.CreateConnection();
            return await connection.QueryAsync<string>(sql, new { windowsId });
        }
    }
}
