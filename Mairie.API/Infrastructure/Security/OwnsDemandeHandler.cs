using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Mairie.Domain.Interfaces;
using System.Security.Claims;

namespace Mairie.API.Infrastructure.Security;

public class OwnsDemandeHandler : AuthorizationHandler<OwnsDemandeRequirement>
{
    private readonly IDemandeRepository _demandeRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OwnsDemandeHandler(IDemandeRepository demandeRepository, IHttpContextAccessor httpContextAccessor)
    {
        _demandeRepository = demandeRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnsDemandeRequirement requirement)
    {
        // Expecting an HTTP request; bail out if not present
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return;

        // Try to get route id (api/demandes/{id})
        if (!httpContext.Request.RouteValues.TryGetValue("id", out var idValue))
            return;

        if (!int.TryParse(idValue?.ToString(), out var demandeId))
            return;

        // Load the Demande from repository (assumes a method like GetByIdAsync exists)
        var demande = await _demandeRepository.GetByIdAsync(demandeId);
        if (demande == null)
            return; // not found => let controller return 404

        var user = httpContext.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return;

        // Allow if the user has an elevated role
        if (user.IsInRole("Administrateur") || user.IsInRole("ChefService"))
        {
            context.Succeed(requirement);
            return;
        }

        // Otherwise check ownership: assumes Demande has a property OwnerId or UserId
        // Adapt the property name if your domain model differs (e.g., CreatedById)
        var claimUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claimUserId))
            return;

        // Compare to domain object's owner identifier (strings compared to be tolerant)
        // Adjust as necessary if your IDs are ints/guid on domain model
        var ownerId = demande.OwnerId?.ToString() ?? demande.UserId?.ToString(); // try common property names
        if (!string.IsNullOrEmpty(ownerId) && ownerId == claimUserId)
        {
            context.Succeed(requirement);
        }
    }
}