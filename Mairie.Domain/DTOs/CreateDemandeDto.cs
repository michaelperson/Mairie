using Mairie.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace Mairie.Domain.DTOs
{
    public class CreateDemandeDto
    {
        [Required(ErrorMessage = "Le Nom du citoyen est obligatoire")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Le nom du citoyen doit contenir entre 3 et 100 caractères")]
        public string NomCitoyen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type de demande est obligatoire")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "La type de deomande doit contenir entre 5 et 100 caractères")]
        public string TypeDeDemande { get; set; } = string.Empty;
 
    }
}
