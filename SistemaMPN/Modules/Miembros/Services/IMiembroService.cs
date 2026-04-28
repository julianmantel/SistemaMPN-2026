using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;
namespace SistemaMPN.Modules.Miembros.Services
{
    public interface IMiembroService
    {
        public Miembro CargarDatosBasicosMiembro(Miembro miembro, InfoBasicaMiembroDTO infoBasicaMiembro);
        public DatoPersonal CargarDatosPersonales(InformacionPersonalDTO informacionPersonal);
        public Trayectoria CargarTrayectoria(InformacionAcademicaDTO informacionAcademica, InformacionLaboralDTO informacionLaboral);
        public InformacionEclesiastica CargarInfoEclesiastica(InformacionEclesiasticaDTO informacionEclesiastica);
        public Bautismo CargarBautismo(InformacionEclesiasticaDTO informacionEclesiastica);
        public InformacionSalud CargarInfoSalud(InformacionSaludDTO informacionSalud);
        public bool TieneInformacionEclesiastica(InformacionEclesiasticaDTO dto);
        public bool TieneInformacionAcademica(InformacionAcademicaDTO dto);
        public bool TieneInformacionLaboral(InformacionLaboralDTO dto);
        public bool TieneInformacionPersonal(InformacionPersonalDTO dto);
        public bool TieneInformacionSalud(InformacionSaludDTO dto);
        public bool TieneDireccion(DireccionDTO dto);
    }
}
