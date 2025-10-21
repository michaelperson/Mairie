using Microsoft.AspNetCore.Authorization;

namespace Mairie.API.Infrastructure.Security
{
    public class OwnsDemandeRequirement : IAuthorizationRequirement
    {
        public string OperationName { get; }

        public OwnsDemandeRequirement(string operationName)
        {
            OperationName = operationName;
        }
    }
}
