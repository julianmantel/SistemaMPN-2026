using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.Models
{
    public partial class Documento
    {
        public int IdDocumento { get; set; }
        public DateOnly Fecha { get; set; }
        public int NroDocumento { get; set; }
        public string Tipo { get; set; }
        public bool Firmado { get; set; } = false;
        public virtual ICollection<DocumentoTesorero> DocumentoTesoreros { get; set; } = new List<DocumentoTesorero>();
        public string NombreArchivoOriginal { get; set; } = string.Empty;
        public string NombreArchivoFirmado { get; set; } = string.Empty;
    }
}
