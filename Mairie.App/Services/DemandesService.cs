using Mairie.App.Services.Interfaces;
using Mairie.Domain.DTOs;
using Mairie.Domain.Entities;
using Mairie.Domain.Enumerations;

namespace Mairie.App.Services
{
    /// <summary>
    /// Service de gestion des demandes avec contrôles de sécurité
    /// </summary>
    public class DemandesService : BaseApiService, IDemandesService
    {
        public DemandesService(
            IHttpClientFactory httpClientFactory,
            ILogger<DemandesService> logger)
            : base(httpClientFactory, logger)
        {
        }

        /// <summary>
        /// Récupère toutes les demandes (Agent et Chef de Service)
        /// </summary>
        public async Task<List<DemandeReadDTO>?> GetDemandesAsync()
        {
            return await GetAsync<List<DemandeReadDTO>>("/api/Demandes");
        }

        /// <summary>
        /// Récupère une demande par son ID
        /// </summary>
        public async Task<DemandeReadDTO?> GetDemandeByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Tentative de récupération d'une demande avec un ID invalide: {Id}", id);
                return null;
            }

            return await GetAsync<DemandeReadDTO>($"/api/Demandes/{id}");
        }

        /// <summary>
        /// Crée une nouvelle demande (Agent uniquement)
        /// </summary>
        public async Task<DemandeReadDTO?> CreateDemandeAsync(CreateDemandeDto demande)
        {
            if (demande == null)
            {
                throw new ArgumentNullException(nameof(demande));
            }

            // Validation supplémentaire côté client (Defense in Depth)
            if (string.IsNullOrWhiteSpace(demande.NomCitoyen) ||
                string.IsNullOrWhiteSpace(demande.TypeDeDemande))
            {
                _logger.LogWarning("Tentative de création d'une demande avec des données invalides");
                return null;
            }

            return await PostAsync<CreateDemandeDto, DemandeReadDTO>("/api/Demandes", demande);
        }

        /// <summary>
        /// Met à jour une demande existante
        /// </summary>
        public async Task<bool> UpdateDemandeAsync(int id, UpdateDemandeDto demande)
        {
            if (id <= 0 || demande == null)
            {
                _logger.LogWarning("Tentative de mise à jour avec des données invalides");
                return false;
            }

            return await PutAsync($"/api/Demandes/{id}", demande);
        }

        /// <summary>
        /// Supprime une demande (Agent - seulement ses propres demandes)
        /// </summary>
        public async Task<bool> DeleteDemandeAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Tentative de suppression avec un ID invalide: {Id}", id);
                return false;
            }

            return await DeleteAsync($"/api/Demandes/{id}");
        }

        /// <summary>
        /// Récupère les demandes par statut
        /// </summary>
        public async Task<List<DemandeReadDTO>?> GetDemandesByStatutAsync(StatutEnum statut)
        {
            return await GetAsync<List<DemandeReadDTO>>($"/api/Demandes/statut/{statut}");
        }

        /// <summary>
        /// Recherche des demandes
        /// </summary>
        public async Task<List<DemandeReadDTO>?> SearchDemandesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetDemandesAsync();
            }

            // Sanitization du terme de recherche (Security by Design)
            var sanitizedTerm = Uri.EscapeDataString(searchTerm.Trim());

            return await GetAsync<List<DemandeReadDTO>>($"/api/Demandes/search?term={sanitizedTerm}");
        }

        /// <summary>
        /// Valide une demande (Chef de Service uniquement)
        /// </summary>
        public async Task<bool> ValiderDemandeAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Tentative de validation avec un ID invalide: {Id}", id);
                return false;
            }

            return await PutAsync($"/api/Demandes/valider/{id}", new { });
        }
    }
}
