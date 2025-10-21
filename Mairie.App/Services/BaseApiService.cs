using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mairie.App.Services
{
    /// <summary>
    /// Service de base pour les appels API avec gestion d'erreurs sécurisée
    /// </summary>
    public abstract class BaseApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;

        protected BaseApiService(IHttpClientFactory httpClientFactory, ILogger logger)
        {
            _httpClient = httpClientFactory.CreateClient("DemandesAPI");
            _logger = logger;
        }

        /// <summary>
        /// Effectue une requête GET avec gestion d'erreurs
        /// </summary>
        protected async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                _logger.LogInformation("GET {Endpoint}", endpoint);

                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    };

                    string responsestr = await response.Content.ReadAsStringAsync();
                    return await response.Content.ReadFromJsonAsync<T>();
                }

                await LogErrorResponse(response, endpoint);
                return default;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erreur réseau lors de l'appel à {Endpoint}", endpoint);
                throw new ApplicationException("Erreur de connexion au serveur", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de l'appel à {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Effectue une requête POST avec gestion d'erreurs
        /// </summary>
        protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                _logger.LogInformation("POST {Endpoint}", endpoint);

                var response = await _httpClient.PostAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TResponse>();
                }

                await LogErrorResponse(response, endpoint);
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du POST à {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Effectue une requête PUT avec gestion d'erreurs
        /// </summary>
        protected async Task<bool> PutAsync<TRequest>(string endpoint, TRequest data)
        {
            try
            {
                _logger.LogInformation("PUT {Endpoint}", endpoint);

                var response = await _httpClient.PutAsJsonAsync(endpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                await LogErrorResponse(response, endpoint);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du PUT à {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Effectue une requête DELETE avec gestion d'erreurs
        /// </summary>
        protected async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                _logger.LogInformation("DELETE {Endpoint}", endpoint);

                var response = await _httpClient.DeleteAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                await LogErrorResponse(response, endpoint);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du DELETE à {Endpoint}", endpoint);
                throw;
            }
        }

        /// <summary>
        /// Log les erreurs de réponse de manière sécurisée (sans exposer de détails sensibles)
        /// </summary>
        private async Task LogErrorResponse(HttpResponseMessage response, string endpoint)
        {
            var statusCode = (int)response.StatusCode;

            // Ne pas logger le contenu complet en production pour éviter la fuite d'informations
            _logger.LogWarning(
                "Échec de la requête à {Endpoint}. Status: {StatusCode}",
                endpoint,
                statusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Accès non autorisé à {Endpoint}", endpoint);
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogError("Accès interdit à {Endpoint}", endpoint);
            }
        }
    }
}