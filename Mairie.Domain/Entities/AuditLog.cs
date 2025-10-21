using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mairie.Domain.Entities
{
    public class AuditLog
    {
       public int Id { get; set; }
        public string WindowsId { get; set; } = string.Empty;
        public string Action { get;set; } = string.Empty;
        public DateTime DateAction { get; set; }
        public string Resultat { get; set; } = string.Empty;
    }
}
