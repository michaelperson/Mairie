namespace Mairie.App.Middleware
{
    /// <summary>
    /// Middleware de sécurité pour implémenter des contrôles supplémentaires
    /// Security by Design: Defense in Depth
    /// </summary>
    public class SecurityMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityMiddleware> _logger;

        public SecurityMiddleware(RequestDelegate next, ILogger<SecurityMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Log toutes les requêtes (pour l'audit)
            _logger.LogInformation(
                "Requête: {Method} {Path} depuis {IP}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            // Vérification de l'authentification Windows
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning(
                    "Tentative d'accès non authentifié: {Path} depuis {IP}",
                    context.Request.Path,
                    context.Connection.RemoteIpAddress);
            }

            // Protection contre les requêtes trop volumineuses (DoS)
            if (context.Request.ContentLength > 10 * 1024 * 1024) // 10 MB
            {
                _logger.LogWarning(
                    "Requête trop volumineuse bloquée: {Size} bytes depuis {IP}",
                    context.Request.ContentLength,
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                return;
            }

            // Validation du Content-Type pour les requêtes POST/PUT
            if (context.Request.Method == "POST" || context.Request.Method == "PUT")
            {
                var contentType = context.Request.ContentType;
                if (!string.IsNullOrEmpty(contentType) &&
                    !contentType.StartsWith("application/json") &&
                    !contentType.StartsWith("multipart/form-data") &&
                    !contentType.StartsWith("application/x-www-form-urlencoded"))
                {
                    _logger.LogWarning(
                        "Content-Type suspect détecté: {ContentType} depuis {IP}",
                        contentType,
                        context.Connection.RemoteIpAddress);
                }
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Extension pour enregistrer le middleware
    /// </summary>
    public static class SecurityMiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SecurityMiddleware>();
        }
    }
}
