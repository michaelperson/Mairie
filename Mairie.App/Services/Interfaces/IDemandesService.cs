using Mairie.Domain.DTOs;
using Mairie.Domain.Entities;
using Mairie.Domain.Enumerations;

namespace Mairie.App.Services.Interfaces
{
    public interface IDemandesService
    {
        Task<List<DemandeReadDTO>?> GetDemandesAsync();
        Task<DemandeReadDTO?> GetDemandeByIdAsync(int id);
        Task<DemandeReadDTO?> CreateDemandeAsync(CreateDemandeDto demande);
        Task<bool> UpdateDemandeAsync(int id, UpdateDemandeDto demande);
        Task<bool> DeleteDemandeAsync(int id);
        Task<List<DemandeReadDTO>?> GetDemandesByStatutAsync(StatutEnum statut);
        Task<List<DemandeReadDTO>?> SearchDemandesAsync(string searchTerm);
        Task<bool> ValiderDemandeAsync(int id);
    }
}
