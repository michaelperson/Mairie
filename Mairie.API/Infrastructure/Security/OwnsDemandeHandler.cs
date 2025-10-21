using Mairie.Domain.Entities;
using Mairie.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Mairie.API.Infrastructure.Security;

public class OwnsDemandeHandler : AuthorizationHandler<OwnsDemandeRequirement, Demande>
{
    private readonly IUserContext _userContext;
    private readonly IUserRoleRepository _userRoleRepository;

    public OwnsDemandeHandler(IUserContext userContext, IUserRoleRepository userRoleRepository)
    {
        _userContext = userContext;
        _userRoleRepository = userRoleRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnsDemandeRequirement requirement,
        Demande resource)
    {
        if (!_userContext.IsAuthenticated)
        {
            context.Fail();
            return;
        }

        string userId = _userContext.WindowsId ?? string.Empty;
        IEnumerable<string> roles = await _userRoleRepository.GetRolesByWindowsIdAsync(userId);

        switch (requirement.Action)
        {
            case "Create":
                if (roles.Contains("Agent") || roles.Contains("ChefService") || roles.Contains("Administrateur"))
                    context.Succeed(requirement);
                break;

            case "Read":
                if (roles.Contains("Administrateur") || roles.Contains("ChefService"))
                    context.Succeed(requirement);
                else if (roles.Contains("Agent") && resource.CreatedByWindowsId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                    context.Succeed(requirement);
                break;

            case "Update":
                if (roles.Contains("Administrateur"))
                    context.Succeed(requirement);
                else if (roles.Contains("Agent") && resource.CreatedByWindowsId.Equals(userId, StringComparison.OrdinalIgnoreCase))
                    context.Succeed(requirement);
                break;

            case "Delete":
                if (roles.Contains("Administrateur"))
                    context.Succeed(requirement);
                break;

            case "Approve":
                if (roles.Contains("ChefService") || roles.Contains("Administrateur"))
                    context.Succeed(requirement);
                break;
        }
    }
}
 