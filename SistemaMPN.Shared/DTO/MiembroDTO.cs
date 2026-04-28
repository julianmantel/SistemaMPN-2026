using SistemaMPN.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class MiembroDTO
    {
        public int id { get; set; }

        public DateTime? fecha_creacion { get; set; }

        [ValidateComplexType]
        public InfoBasicaMiembroDTO info_basica_miembro { get; set; } = new();

        [ValidateComplexType]
        public InformacionSaludDTO info_salud { get; set; } = new();

        [ValidateComplexType]
        public InformacionAcademicaDTO info_academica { get; set; } = new();

        [ValidateComplexType]
        public InformacionLaboralDTO info_laboral { get; set; } = new();

        [ValidateComplexType]
        public InformacionEclesiasticaDTO info_eclesiastica { get; set; } = new();

        [ValidateComplexType]
        public InformacionPersonalDTO info_personal { get; set; } = new();

        [ValidateComplexType]
        public DireccionDTO direccion { get; set; } = new();
    }
}
