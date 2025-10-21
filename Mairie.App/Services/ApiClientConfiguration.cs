using System.Net;

namespace Mairie.Appp.Services;

/// <summary>
/// Configuration sécurisée du HttpClient pour l'authentification Windows
/// </summary>
public static class ApiClientConfiguration
{
    public static void ConfigureHttpClient(IServiceCollection services, IConfiguration configuration)
    {
        var apiBaseUrl = configuration["ApiSettings:BaseUrl"]
            ?? throw new InvalidOperationException("L'URL de l'API doit être configurée");

        // Configuration du HttpClient avec authentification Windows
        services.AddHttpClient("DemandesAPI", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(
                configuration.GetValue<int>("ApiSettings:Timeout", 30));
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // Activation de l'authentification Windows
            UseDefaultCredentials = true,
            PreAuthenticate = true,

            // IMPORTANT : Permettre les redirections automatiques
            AllowAutoRedirect = true,

            // Configuration SSL/TLS (Security by Design)
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                // EN DÉVELOPPEMENT : accepter tous les certificats
                // EN PRODUCTION : valider correctement le certificat
                return true;
            },

            // Configuration des credentials avec domaine
            UseProxy = false,
            Credentials = CredentialCache.DefaultNetworkCredentials,

            // Support des cookies pour l'authentification
            UseCookies = true,
            CookieContainer = new System.Net.CookieContainer()
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));
    }
}