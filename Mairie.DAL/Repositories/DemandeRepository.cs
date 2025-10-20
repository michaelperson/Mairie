using Dapper;
using Mairie.DAL.Configuration;
using Mairie.Domain.Entities;
using Mairie.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.DAL.Repositories
{
    public class DemandeRepository : IDemandeRepository
    {
        private readonly DatabaseConfiguration _dbConfig;

        public DemandeRepository(DatabaseConfiguration dbConfig)
        {
            _dbConfig = dbConfig ?? throw new ArgumentNullException(nameof(dbConfig));
        }

        public async Task<IEnumerable<Demande>> GetAllAsync()
        {
            const string sql = @"
            SELECT 
                Id, 
                NomCitoyen, 
                TypeDemande, 
                Statut 
            FROM Demande
            ORDER BY DateCreation DESC";

            using var connection = _dbConfig.CreateConnection();
            return await connection.QueryAsync<Demande>(sql);
        }

        public async Task<Demande?> GetByIdAsync(int id)
        {
            const string sql = @"
            SELECT 
                Id, 
                NomCitoyen, 
                TypeDemande, 
                Statut 
            FROM Demande
            WHERE Id = @Id";

            using var connection = _dbConfig.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Demande>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(Demande demande)
        {
            const string sql = @"
            INSERT INTO Demande 
                (NomCitoyen, TypeDemande, Statut, DateCreation )
            VALUES 
                (@NomCitoyen, @TypeDemande, @Statut, @DateCreation);
            
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = _dbConfig.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                demande.NomCitoyen,
                demande.TypeDemande,
                demande.Statut,
                demande.DateCreation, 
            });
        }

        public async Task<bool> UpdateAsync(Demande demande)
        {
            const string sql = @"
            UPDATE Demande
            SET 
                NomCitoyen = @NomCitoyen,
                TypeDemande = @TypeDemande,
                Statut = @Statut 
            WHERE Id = @Id";

            using var connection = _dbConfig.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                demande.Id,
                demande.NomCitoyen,
                demande.TypeDemande,
                demande.Statut 
            });

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = @"
            DELETE FROM Demande
            WHERE Id = @Id";

            using var connection = _dbConfig.CreateConnection();
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Demande>> GetByStatutAsync(string statut)
        {
            const string sql = @"
            SELECT 
                Id, 
                NomCitoyen, 
                TypeDemande, 
                Statut 
            FROM Demande
            WHERE Statut = @Statut
            ORDER BY DateCreation DESC";

            using var connection = _dbConfig.CreateConnection();
            return await connection.QueryAsync<Demande>(sql, new { Statut = statut });
        }

        public async Task<IEnumerable<Demande>> SearchAsync(string searchTerm)
        {
            const string sql = @"
            SELECT 
                Id, 
                NomCitoyen, 
                TypeDemande, 
                Statut 
            FROM Demande
            WHERE 
                NomCitoyen LIKE @SearchPattern 
                OR TypeDemande LIKE @SearchPattern 
            ORDER BY DateCreation DESC";

            using var connection = _dbConfig.CreateConnection();
            var searchPattern = $"%{searchTerm}%";
            return await connection.QueryAsync<Demande>(sql, new { SearchPattern = searchPattern });
        }
    }
}
