using Mairie.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Mairie.API.Infrastructure.Security
{
    public class RoleClaimsTransformation : IClaimsTransformation
    {
        private readonly IUserContext _userContext;
        private readonly IUserRoleRepository _repo;

        public RoleClaimsTransformation(IUserContext userContext, IUserRoleRepository repo)
        {
            _userContext = userContext;
            _repo = repo;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is not { IsAuthenticated: true }) return principal;

            var roles = await _repo.GetRolesByWindowsIdAsync(_userContext.WindowsId);
            var identity = (ClaimsIdentity)principal.Identity;

            foreach (var role in roles)
                identity.AddClaim(new Claim(ClaimTypes.Role, role));

            return principal;
        }
    }
}
