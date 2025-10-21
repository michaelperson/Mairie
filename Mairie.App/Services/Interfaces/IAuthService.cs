using Mairie.Domain.DTOs;
using Mairie.Domain.Enumerations;

namespace Mairie.App.Services.Interfaces
{
    public interface IAuthService
    {
        Task<UserInfoDTO?> GetCurrentUserAsync();
        Task<bool> HasRoleAsync(RoleEnum role);
        Task<bool> IsAgentAsync();
        Task<bool> IsChefServiceAsync();
        Task<bool> IsAdministrateurAsync();
    }
}
