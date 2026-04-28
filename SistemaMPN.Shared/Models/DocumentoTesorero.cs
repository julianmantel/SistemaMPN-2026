using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.Models
{
    public class DocumentoTesorero
    {
        public int IdDocumento { get; set; }
        public int IdTesorero { get; set; }
        public bool EsCreador { get; set; }
        public bool EsSupervisor { get; set; }
        public Documento IdDocumentoNavigation { get; set; }
        public Tesorero IdTesoreroNavigation { get; set; }
    }
}
