using Microsoft.AspNetCore.Authorization;

namespace Mairie.API.Infrastructure.Security
{
    public class OwnsDemandeRequirement : IAuthorizationRequirement
    {
        public string Action { get; }

        public OwnsDemandeRequirement(string action)
        {
            Action = action;
        }
    }
}
