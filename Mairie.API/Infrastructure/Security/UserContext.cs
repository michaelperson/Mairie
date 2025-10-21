using Mairie.Domain.Interfaces;
using System.Security.Principal;

namespace Mairie.API.Infrastructure.Security
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserContext> _logger;

        public UserContext(IHttpContextAccessor accessor, ILogger<UserContext> logger)
        {
            _httpContextAccessor = accessor;
            _logger = logger;
        }

#pragma warning disable CA1416 // Validate platform compatibility
        public string WindowsId
        {
            get
            {
                if (_httpContextAccessor.HttpContext?.User?.Identity is WindowsIdentity windowsIdentity)
                {
                    return windowsIdentity.User?.Value ?? string.Empty;
                }
                return string.Empty;
            }
        }
             
#pragma warning restore CA1416 // Validate platform compatibility

        public string? DisplayName
        {
            get {
                var user = _httpContextAccessor.HttpContext?.User;

                if (user?.Identity is WindowsIdentity windowsIdentity)
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    _logger.LogInformation("WindowsIdentity trouvée : {Name}", windowsIdentity.Name);
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning disable CA1416 // Validate platform compatibility
                    return windowsIdentity.Name ?? string.Empty;
#pragma warning restore CA1416 // Validate platform compatibility
                }

                // Fallback sur l'identity standard
                var username = user?.Identity?.Name ?? string.Empty;
                _logger.LogWarning("WindowsIdentity non trouvée, utilisation de Identity.Name : {Username}", username);
                return username;
            }
        }

        public bool IsAuthenticated =>
            _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}
