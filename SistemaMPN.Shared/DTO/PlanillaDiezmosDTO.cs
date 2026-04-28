using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class PlanillaDiezmosDTO
    {
        public string Fecha { get; set; } = "";

        [Required(ErrorMessage = "El número de planilla es obligatorio")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El número de planilla debe contener solo dígitos")]
        public string Numero { get; set; } = "";

        [MaxLength(200, ErrorMessage = "No puede superar los 200 caracteres")]
        public string TextoEntrega { get; set; } = "";

        public string NombreSupervisor { get; set; } = "";

        public string NombreTesorero { get; set; } = "";
        public SupervisorDTO Supervisor { get; set; } = new();

        public List<RegistroPersonaDTO> Registros { get; set; } = new();
    }
}
