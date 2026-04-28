using SistemaMPN.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class ReunionDTO
    {
        public int IdReunion { get; set; }

        public DateTime? Fecha { get; set; }

        [MaxLength(300, ErrorMessage = "No puede superar los 300 caracteres")]
        public string? Motivo { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string? Correo { get; set; }
        public string? Estado { get; set; }
        public int IdMiembro { get; set; }
        public int IdConsultor { get; set; }

        // Propiedades adicionales opcionales para mostrar información relacionada
        public string? NombreMiembro { get; set; }
        public string? NombreConsultor { get; set; }

        // ID de petición de reunión (cuando viene de una solicitud de líder)
        public int? IdPeticion { get; set; }
    }
}
