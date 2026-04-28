using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class PeticionCambioDTO
    {
        public int? IdGrupos { get; set; }

        public int? IdMiembros { get; set; }

        public PeticionDTO Peticion { get; set; } = new PeticionDTO { Tipo = "Cambio" };
    }
}
