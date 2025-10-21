using Mairie.App.Services.Interfaces;
using Mairie.Domain.DTOs;
using Mairie.Domain.Enumerations;
using Microsoft.AspNetCore.Components;

namespace Mairie.App.Components
{
    /// <summary>
    /// Composant de base avec vérification des autorisations (Security by Design)
    /// </summary>
    public abstract class AuthorizedComponentBase : ComponentBase
    {
        [Inject]
        protected IAuthService AuthService { get; set; } = default!;

        [Inject]
        protected NavigationManager Navigation { get; set; } = default!;

        [Inject]
        protected ILogger<AuthorizedComponentBase> Logger { get; set; } = default!;

        protected UserInfoDTO? CurrentUser { get; private set; }
        protected bool IsLoading { get; set; } = true;
        protected string? ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadUserInfoAsync();
                await OnUserLoadedAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erreur lors du chargement des informations utilisateur");
                ErrorMessage = "Impossible de charger les informations utilisateur";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Charge les informations de l'utilisateur
        /// </summary>
        protected virtual async Task LoadUserInfoAsync()
        {
            CurrentUser = await AuthService.GetCurrentUserAsync();

            if (CurrentUser == null)
            {
                Logger.LogWarning("Aucun utilisateur connecté");
                ErrorMessage = "Vous devez être connecté pour accéder à cette page";
            }
        }

        /// <summary>
        /// Méthode appelée après le chargement de l'utilisateur
        /// </summary>
        protected virtual Task OnUserLoadedAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Vérifie si l'utilisateur a l'autorisation requise
        /// </summary>
        protected bool HasRequiredRole(params RoleEnum[] roles)
        {
            if (CurrentUser == null) return false;

            return roles.Any(role => CurrentUser.HasRole(role));
        }

        /// <summary>
        /// Redirige vers une page si l'utilisateur n'a pas le rôle requis
        /// </summary>
        protected void RequireRole(RoleEnum role, string redirectUrl = "/")
        {
            if (!HasRequiredRole(role))
            {
                Logger.LogWarning(
                    "Accès refusé: l'utilisateur {Username} n'a pas le rôle {Role}",
                    CurrentUser?.UserName ?? "Inconnu",
                    role);

                Navigation.NavigateTo(redirectUrl);
            }
        }

        /// <summary>
        /// Affiche un message d'erreur sécurisé (sans détails sensibles)
        /// </summary>
        protected void ShowError(string message)
        {
            ErrorMessage = message;
            Logger.LogWarning("Erreur affichée à l'utilisateur: {Message}", message);
            StateHasChanged();
        }

        /// <summary>
        /// Efface le message d'erreur
        /// </summary>
        protected void ClearError()
        {
            ErrorMessage = null;
            StateHasChanged();
        }
    }
}