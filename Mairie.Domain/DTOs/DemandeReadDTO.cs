using Mairie.Domain.Enumerations;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.Domain.DTOs
{
    public class DemandeReadDTO
    {
        [SwaggerSchema("Identifiant unique de la demande")]
        public int Id { get; set; }
        [SwaggerSchema("Nom du citoyen qui a fait la demande")]
        public string NomCitoyen { get; set; } = string.Empty;
        [SwaggerSchema("Type de demande faite par le citoyen")]
        public string TypeDemande { get; set; } = string.Empty;
        [SwaggerSchema("Statut :EnAttente, EnCours, Terminee, Annulee")]
        public StatutEnum Statut { get; set; } = 0;
    }
}
