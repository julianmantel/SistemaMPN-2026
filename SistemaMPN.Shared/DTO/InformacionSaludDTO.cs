using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Shared.DTO
{
    public class InformacionSaludDTO
    {
        public int? id { get; set; }

        [MaxLength(20, ErrorMessage = "No puede superar los 20 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string grupo_sanguineo { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "No puede superar los 500 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string observaciones { get; set; } = string.Empty;

        [Length(0, 14, ErrorMessage = "La lista no puede superar los 14 elementos")]
        public List<string> alergias { get; set; } = new List<string>();

        [Length(0, 40, ErrorMessage = "La lista no puede superar los 40 elementos")]
        public List<string> condiciones_medicas { get; set; } = new List<string>();

        [Length(0, 50, ErrorMessage = "La lista no puede superar los 50 elementos")]
        public List<string> medicamentos { get; set; } = new List<string>();

        public List<TelefonoEmergenciaDTO> telefonosEmergencias { get; set; } = new List<TelefonoEmergenciaDTO>();
        public bool tiene_informacion_salud { get; set; } = false;
    }
}
