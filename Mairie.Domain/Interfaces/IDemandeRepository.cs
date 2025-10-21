using Mairie.Domain.Entities;
using Mairie.Domain.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.Domain.Interfaces
{
    public interface IDemandeRepository
    {
        Task<IEnumerable<Demande>> GetAllAsync();
        Task<Demande?> GetByIdAsync(int id);
        Task<int> CreateAsync(Demande demande);
        Task<bool> UpdateAsync(Demande demande);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Demande>> GetByStatutAsync(StatutEnum statut);
        Task<IEnumerable<Demande>> SearchAsync(string searchTerm);
    }
}
