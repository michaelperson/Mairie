using Mairie.App.Services.Interfaces;
using Mairie.Domain.DTOs;
using Mairie.Domain.Enumerations;

namespace Mairie.App.Services
{
    /// <summary>
    /// Service d'authentification et d'autorisation
    /// L'API mappe l'utilisateur Windows aux rôles de la base de données
    /// </summary>
    public class AuthService : BaseApiService, IAuthService
    {
        private UserInfoDTO? _cachedUserInfo;
        private DateTime _cacheExpiration;
        private readonly TimeSpan _cacheLifetime = TimeSpan.FromMinutes(5);

        public AuthService(
            IHttpClientFactory httpClientFactory,
            ILogger<AuthService> logger)
            : base(httpClientFactory, logger)
        {
        }

        /// <summary>
        /// Récupère les informations de l'utilisateur connecté via l'API
        /// L'API utilise l'authentification Windows pour identifier l'utilisateur
        /// </summary>
        public async Task<UserInfoDTO?> GetCurrentUserAsync()
        {
            // Utilisation du cache pour éviter les appels répétés
            if (_cachedUserInfo != null && DateTime.UtcNow < _cacheExpiration)
            {
                return _cachedUserInfo;
            }

            try
            {
                // L'API récupère automatiquement l'utilisateur Windows
                // et retourne ses informations avec les rôles associés
                var userInfo = await GetAsync<UserInfoDTO>("/api/Auth/current-user");

                if (userInfo != null)
                {
                    _cachedUserInfo = userInfo;
                    _cacheExpiration = DateTime.UtcNow.Add(_cacheLifetime);

                    _logger.LogInformation(
                        $"Utilisateur récupéré: {userInfo.UserName} avec {userInfo.Roles.Count()} rôle(s)"
                        );
                }
                else
                {
                    _logger.LogWarning("Impossible de récupérer les informations de l'utilisateur");
                }

                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur");
                return null;
            }
        }

        /// <summary>
        /// Vérifie si l'utilisateur possède un rôle spécifique
        /// </summary>
        public async Task<bool> HasRoleAsync(RoleEnum role)
        {
            var user = await GetCurrentUserAsync();
            return user?.HasRole(role) ?? false;
        }

        /// <summary>
        /// Vérifie si l'utilisateur est un Agent
        /// </summary>
        public async Task<bool> IsAgentAsync()
        {
            return await HasRoleAsync(RoleEnum.Agent);
        }

        /// <summary>
        /// Vérifie si l'utilisateur est un Chef de Service
        /// </summary>
        public async Task<bool> IsChefServiceAsync()
        {
            return await HasRoleAsync(RoleEnum.ChefService);
        }

        /// <summary>
        /// Vérifie si l'utilisateur est un Administrateur
        /// </summary>
        public async Task<bool> IsAdministrateurAsync()
        {
            return await HasRoleAsync(RoleEnum.Administrateur);
        }

        /// <summary>
        /// Invalide le cache de l'utilisateur (utile après une modification de rôle)
        /// </summary>
        public void InvalidateCache()
        {
            _cachedUserInfo = null;
            _cacheExpiration = DateTime.MinValue;
            _logger.LogInformation("Cache utilisateur invalidé");
        }
    }
}
