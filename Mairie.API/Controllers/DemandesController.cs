using Mairie.API.DTOs;
using Mairie.API.Infrastructure.Security;
using Mairie.Domain.Entities;
using Mairie.Domain.Enumerations;
using Mairie.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Mairie.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DemandesController : ControllerBase
    {
        private readonly IAuthorizationService _authService;
        private readonly IDemandeRepository _repository;
        private readonly ILogger<DemandesController> _logger;

        public DemandesController(IAuthorizationService authService, IDemandeRepository repository, ILogger<DemandesController> logger)
        {
            _authService = authService;
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerResponse(200, "Liste des demandes", typeof(IEnumerable<DemandeReadDTO>))]
        [SwaggerResponse(404, "Aucune demande trouvée")]
        [SwaggerResponse(500, "Erreur lors de la récupération des données")]
        public async Task<ActionResult<IEnumerable<Demande>>> GetAll()
        {
            try
            {
                var demandes = await _repository.GetAllAsync();
                return Ok(demandes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des demandes");
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
    Summary = "Récupère une demande par son identifiant",
    Description = "Retourne les informations détaillées d’une demande d'un citoyen de la mairie."
)]
        [SwaggerResponse(200, "La demande identifiée par l'id", typeof(DemandeReadDTO))]
        [SwaggerResponse(404, "Aucune demande trouvée avec l'id transmis")]
        [SwaggerResponse(500, "Erreur lors de la récupération des données")]
        public async Task<ActionResult<Demande>> GetById([SwaggerParameter("Identifiant de la demande", Required = true)] int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID invalide");
            }

            try
            {
                var demande = await _repository.GetByIdAsync(id);
                if (demande == null)
                {
                    return NotFound($"Demande avec l'ID {id} introuvable");
                }
                /// Vérification des autorisations
                var result = await _authService.AuthorizeAsync(User, demande, DemandeRequirement.Read);

                if (!result.Succeeded)
                    return Forbid();

                return Ok(demande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de la demande {Id}", id);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        [HttpGet("statut/{statut}")]
        public async Task<ActionResult<IEnumerable<Demande>>> GetByStatut(StatutEnum statut)
        {
             

            try
            {
                var demandes = await _repository.GetByStatutAsync(statut);
                return Ok(demandes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des demandes par statut {Statut}", statut);
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Demande>>> Search([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 3)
            {
                return BadRequest("Le terme de recherche doit contenir au moins 3 caractères");
            }

            try
            {
                var demandes = await _repository.SearchAsync(term);
                return Ok(demandes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche de demandes");
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        [Authorize(Policy = "AdminOrAgent")]

        [HttpPost]
        public async Task<ActionResult<Demande>> Create([FromBody] CreateDemandeDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var demande = new Demande
                {
                    NomCitoyen = dto.NomCitoyen.Trim(),
                    TypeDemande = dto.TypeDeDemande.Trim(),
                    Statut = "EnAttente",
                    DateCreation = DateTime.Now,
                    CreatedByWindowsId = User.Identity?.Name ?? "Inconnu"
                };

                var id = await _repository.CreateAsync(demande);
                demande.Id = id;

                return CreatedAtAction(nameof(GetById), new { id }, demande);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de la demande");
                return StatusCode(500, "Une erreur est survenue");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateDemandeDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans le corps");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingDemande = await _repository.GetByIdAsync(id);
                if (existingDemande == null)
                {
                    return NotFound($"Demande avec l'ID {id} introuvable");
                }

                var demande = new Demande
                {
                    Id = dto.Id,
                    NomCitoyen = dto.NomCitoyen.Trim(),
                    TypeDemande = dto.TypeDeDemande.Trim(),
                    Statut = existingDemande.Statut,
                    DateCreation = existingDemande.DateCreation
                };

                var success = await _repository.UpdateAsync(demande);
                if (!success)
                {
                    return StatusCode(500, "Échec de la mise à jour");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la demande {Id}", id);
                return StatusCode(500, "Une erreur est survenue");
            }
        }


        [Authorize(Policy = "ChefService")]
        [HttpPut("valider")]
        public async Task<ActionResult> ValiderDemande(int id, [FromBody] UpdateDemandeDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("L'ID dans l'URL ne correspond pas à l'ID dans le corps");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var existingDemande = await _repository.GetByIdAsync(id);
                if (existingDemande == null)
                {
                    return NotFound($"Demande avec l'ID {id} introuvable");
                }

                var demande = new Demande
                {
                    Id = dto.Id,
                    NomCitoyen = existingDemande.NomCitoyen,
                    TypeDemande = existingDemande.TypeDemande,
                    Statut = "Validé",
                    DateCreation = existingDemande.DateCreation
                };

                var success = await _repository.UpdateAsync(demande);
                if (!success)
                {
                    return StatusCode(500, "Échec de la mise à jour");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la mise à jour de la demande {Id}", id);
                return StatusCode(500, "Une erreur est survenue");
            }
        }



        [Authorize(Policy = "Administrateur")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest("ID invalide");
            }

            try
            {
                var existingDemande = await _repository.GetByIdAsync(id);
                if (existingDemande == null)
                {
                    return NotFound($"Demande avec l'ID {id} introuvable");
                }

                var success = await _repository.DeleteAsync(id);
                if (!success)
                {
                    return StatusCode(500, "Échec de la suppression");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de la demande {Id}", id);
                return StatusCode(500, "Une erreur est survenue");
            }
        }
    }
}
