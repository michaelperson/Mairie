using Mairie.App.Services.Interfaces;
using Mairie.Domain.DTOs; 
using Mairie.Domain.Enumerations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Data;

namespace Mairie.App.Components.Pages.Demandes
{
    public partial class ListeDemandes
    {
        [Inject] private IDemandesService DemandesService { get; set; } = default!;

        private List<DemandeReadDTO>? demandes;
        private List<DemandeReadDTO>? filteredDemandes;
        private string searchTerm = string.Empty;
        private string selectedStatut = string.Empty;

        protected override async Task OnUserLoadedAsync()
        {
            // Vérifier que l'utilisateur a au moins le rôle Agent ou ChefService
            if (!HasRequiredRole(RoleEnum.Agent, RoleEnum.ChefService))
            {
                ShowError("Vous n'avez pas les droits pour accéder à cette page.");
                return;
            }

            await LoadDemandesAsync();
        }

        private async Task LoadDemandesAsync()
        {
            try
            {
                IsLoading = true;
                StateHasChanged();

                demandes = await DemandesService.GetDemandesAsync();
                ApplyFilters();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erreur lors du chargement des demandes");
                ShowError("Erreur lors du chargement des demandes");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        private void ApplyFilters()
        {
            if (demandes == null)
            {
                filteredDemandes = null;
                return;
            }

            filteredDemandes = demandes;

            // Filtre par terme de recherche
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                filteredDemandes = filteredDemandes.Where(d =>
                    d.NomCitoyen.ToLower().Contains(term) ||
                    d.TypeDemande.ToLower().Contains(term) ||
                    d.Statut.ToString().Contains(term)).ToList();
            }

            // Filtre par statut
            if (!string.IsNullOrEmpty(selectedStatut) &&
                Enum.TryParse<StatutEnum>(selectedStatut, out var statut))
            {
                filteredDemandes = filteredDemandes.Where(d => d.Statut == statut).ToList();
            }
            else
            {
                filteredDemandes = filteredDemandes.ToList();
            }
        }

        private void NavigateToCreate()
        {
            Navigation.NavigateTo("/demandes/create");
        }

        private void ViewDetails(int id)
        {
            Navigation.NavigateTo($"/demandes/{id}");
        }

        private void EditDemande(int id)
        {
            Navigation.NavigateTo($"/demandes/edit/{id}");
        }

        private async Task DeleteDemande(int id)
        {
            if (!await JSRuntime.InvokeAsync<bool>("confirm", "Êtes-vous sûr de vouloir supprimer cette demande ?"))
                return;

            try
            {
                var success = await DemandesService.DeleteDemandeAsync(id);
                if (success)
                {
                    await LoadDemandesAsync();
                }
                else
                {
                    ShowError("Impossible de supprimer la demande");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erreur lors de la suppression de la demande {Id}", id);
                ShowError("Erreur lors de la suppression");
            }
        }

        private async Task ValiderDemande(int id)
        {
            try
            {
                var success = await DemandesService.ValiderDemandeAsync(id);
                if (success)
                {
                    await LoadDemandesAsync();
                }
                else
                {
                    ShowError("Impossible de valider la demande");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Erreur lors de la validation de la demande {Id}", id);
                ShowError("Erreur lors de la validation");
            }
        }

        private async Task RefuserDemande(int id)
        {
            // TODO: Implémenter la logique de refus avec commentaire
            ShowError("Fonctionnalité à implémenter");
        }

        private string GetStatutBadgeClass(StatutEnum statut)
        {
            return statut switch
            {
                StatutEnum.EnAttente => "bg-warning",
                StatutEnum.Terminee => "bg-success",
                StatutEnum.Annulee => "bg-danger", 
                StatutEnum.EnCours => "bg-secondary",
                _ => "bg-secondary"
            };
        }

        private string GetStatutLabel(StatutEnum statut)
        {
            return statut switch
            {
                StatutEnum.EnAttente => "En Attente", 
                StatutEnum.Annulee => "Annulee",
                StatutEnum.EnCours => "En Cours",
                StatutEnum.Terminee => "Terminée",
                _ => statut.ToString()
            };
        }

        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    }
}
