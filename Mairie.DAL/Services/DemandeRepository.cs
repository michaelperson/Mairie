using Dapper;
using Mairie.DAL.Configuration;
using Mairie.Domain.Entities;
using Mairie.Domain.Enumerations;
using Mairie.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.DAL.Services
{
    public class DemandeRepository :BaseRepository, IDemandeRepository
    {
        public DemandeRepository(DatabaseConfiguration dbConfig) : base(dbConfig) { }

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
                Statut , CreatedByWindowsId
            FROM Demande
            WHERE Id = @Id";

            using var connection = _dbConfig.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Demande>(sql, new { Id = id });
        }

        public async Task<int> CreateAsync(Demande demande)
        {
            const string sql = @"
            INSERT INTO Demande 
                (NomCitoyen, TypeDemande, Statut, DateCreation, CreatedByWindowsId )
            VALUES 
                (@NomCitoyen, @TypeDemande, @Statut, @DateCreation, @CreatedByWindowsId);
            
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = _dbConfig.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                demande.NomCitoyen,
                demande.TypeDemande,
                demande.Statut,
                demande.DateCreation,demande.CreatedByWindowsId 
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

        public async Task<IEnumerable<Demande>> GetByStatutAsync(StatutEnum statut)
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
            return await connection.QueryAsync<Demande>(sql, new { Statut = statut.ToString() });
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
