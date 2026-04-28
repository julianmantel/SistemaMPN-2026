using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class EstadisticasDTO
    {
        public List<RangoEdadesDTO> RangoEdades { get; set; } = new();
        public List<OcupacionGrupoDTO> OcupacionGrupos { get; set; } = new();
        public List<MiembrosPorSexoDto> MiembrosPorSexo { get; set; } = new();
        public List<BautismoEstadoDTO> BautismoEstados { get; set; } = new();
        public List<MiembrosPorFechaCreacionDto> MiembrosPorFechaCreacion { get; set; } = new();
    }
}
