using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Miembros.Services
{
    public class MiembroService : IMiembroService
    {
        public bool TieneInformacionEclesiastica(InformacionEclesiasticaDTO dto)
        {
            return !string.IsNullOrWhiteSpace(dto.convocante) ||
                   dto.fecha_desde_la_que_asiste.HasValue ||
                   dto.fecha_de_bautismo.HasValue ||
                   (dto.realizo_bautismo.HasValue && dto.realizo_bautismo.Value != false) ||
                   !string.IsNullOrWhiteSpace(dto.pastor) ||
                   !string.IsNullOrWhiteSpace(dto.lugar_bautismo) ||
                   (dto.seminarios != null && dto.seminarios.Any());
        }
        public bool TieneInformacionAcademica(InformacionAcademicaDTO dto)
        {
            return !string.IsNullOrWhiteSpace(dto.estudios_primario) ||
                   !string.IsNullOrWhiteSpace(dto.estudios_secundario) ||
                   !string.IsNullOrWhiteSpace(dto.estudios_terciario) ||
                   !string.IsNullOrWhiteSpace(dto.estudios_universitario) ||
                   !string.IsNullOrWhiteSpace(dto.carrera) ||
                   !string.IsNullOrWhiteSpace(dto.cursos_realizados);
        }
        public bool TieneInformacionLaboral(InformacionLaboralDTO dto)
        {
            return !string.IsNullOrWhiteSpace(dto.situacion_laboral) ||
                   !string.IsNullOrWhiteSpace(dto.rubro);
        }
        public bool TieneInformacionPersonal(InformacionPersonalDTO dto)
        {
            return !string.IsNullOrWhiteSpace(dto.telefono_alternativo) ||
                   !string.IsNullOrWhiteSpace(dto.estado_civil) ||
                   !string.IsNullOrWhiteSpace(dto.pareja) ||
                   (dto.hijos != null && dto.hijos.Any());
        }
        public bool TieneInformacionSalud(InformacionSaludDTO dto)
        {
            return !string.IsNullOrWhiteSpace(dto.grupo_sanguineo) ||
                   !string.IsNullOrWhiteSpace(dto.observaciones) ||
                   (dto.alergias != null && dto.alergias.Any()) ||
                   (dto.condiciones_medicas != null && dto.condiciones_medicas.Any()) ||
                   (dto.medicamentos != null && dto.medicamentos.Any()) ||
                   (dto.telefonosEmergencias != null && dto.telefonosEmergencias.Any());
        }
        public bool TieneDireccion(DireccionDTO dto)
        {
            return !string.IsNullOrWhiteSpace(dto.calle) ||
                   dto.altura.HasValue ||
                   !string.IsNullOrWhiteSpace(dto.barrio);
        }

        public Miembro CargarDatosBasicosMiembro(Miembro miembro, InfoBasicaMiembroDTO infoBasicaMiembro)
        {
            miembro.Dni = infoBasicaMiembro.dni;
            miembro.Nombre = infoBasicaMiembro.nombre;
            miembro.Apellido = infoBasicaMiembro.apellido;
            miembro.Nacionalidad = infoBasicaMiembro.nacionalidad;
            miembro.LugarNacimiento = infoBasicaMiembro.lugarNacimiento;
            miembro.Sexo = infoBasicaMiembro.sexo;
            if (infoBasicaMiembro.fecha_nacimiento != null)
            {
                miembro.FechaNacimiento = DateOnly.FromDateTime(infoBasicaMiembro.fecha_nacimiento.Value);
            }
            else
            {
                throw new ArgumentNullException("Fecha de nacimiento no puede ser nula");
            }
            miembro.Telefono = infoBasicaMiembro.telefono;
            if (!string.IsNullOrEmpty(infoBasicaMiembro.telefono_fijo))
            {
                miembro.TelefonoFijo = infoBasicaMiembro.telefono_fijo;
            }
            miembro.FechaCreacion = DateTime.UtcNow;
            return miembro;
        }

        public DatoPersonal CargarDatosPersonales(InformacionPersonalDTO informacionPersonal)
        {
            var datosPersonales = new DatoPersonal();
            datosPersonales.Pareja = string.IsNullOrWhiteSpace(informacionPersonal.pareja) ? null : informacionPersonal.pareja;
            datosPersonales.TelefonoAlternativo = string.IsNullOrWhiteSpace(informacionPersonal.telefono_alternativo) ? null : informacionPersonal.telefono_alternativo;
            datosPersonales.EstadoCivil = string.IsNullOrWhiteSpace(informacionPersonal.estado_civil) ? string.Empty : informacionPersonal.estado_civil;
            return (datosPersonales);
        }

        public Trayectoria CargarTrayectoria(InformacionAcademicaDTO informacionAcademica, InformacionLaboralDTO informacionLaboral)
        {
            var trayectoria = new Trayectoria();
            trayectoria.EstudiosPrimario = string.IsNullOrWhiteSpace(informacionAcademica.estudios_primario) ? string.Empty : informacionAcademica.estudios_primario;
            trayectoria.EstudiosSecundario = string.IsNullOrWhiteSpace(informacionAcademica.estudios_secundario) ? string.Empty : informacionAcademica.estudios_secundario;
            trayectoria.EstudiosTerciario = string.IsNullOrWhiteSpace(informacionAcademica.estudios_terciario) ? string.Empty : informacionAcademica.estudios_terciario;
            trayectoria.EstudiosUniversitario = string.IsNullOrWhiteSpace(informacionAcademica.estudios_universitario) ? string.Empty : informacionAcademica.estudios_universitario;
            trayectoria.Carrera = string.IsNullOrWhiteSpace(informacionAcademica.carrera) ? null : informacionAcademica.carrera;
            trayectoria.CursosRealizados = string.IsNullOrWhiteSpace(informacionAcademica.cursos_realizados) ? null : informacionAcademica.cursos_realizados;

            trayectoria.Rubro = string.IsNullOrWhiteSpace(informacionLaboral.rubro) ? null : informacionLaboral.rubro;
            trayectoria.SituacionLaboral = string.IsNullOrWhiteSpace(informacionLaboral.situacion_laboral) ? null : informacionLaboral.situacion_laboral;

            return trayectoria;
        }

        public InformacionEclesiastica CargarInfoEclesiastica(InformacionEclesiasticaDTO informacionEclesiastica)
        {
            var infoEclesiastica = new InformacionEclesiastica();
            infoEclesiastica.Convocante = string.IsNullOrWhiteSpace(informacionEclesiastica.convocante) ? null : informacionEclesiastica.convocante;
            if (informacionEclesiastica.fecha_desde_la_que_asiste.HasValue)
            {
                infoEclesiastica.FechaAsiste = informacionEclesiastica.fecha_desde_la_que_asiste.Value;
            }

            return infoEclesiastica;
        }

        public Bautismo CargarBautismo(InformacionEclesiasticaDTO informacionEclesiastica)
        {
            var bautismo = new Bautismo();
            bautismo.Realizo = informacionEclesiastica.realizo_bautismo;
            bautismo.Pastor = informacionEclesiastica.pastor;
            bautismo.Lugar = string.IsNullOrWhiteSpace(informacionEclesiastica.lugar_bautismo) ? null : informacionEclesiastica.lugar_bautismo;
            if (informacionEclesiastica.fecha_de_bautismo.HasValue)
            {
                bautismo.Fecha = informacionEclesiastica.fecha_de_bautismo.Value;
            }
            return bautismo;
        }

        public InformacionSalud CargarInfoSalud(InformacionSaludDTO informacionSalud)
        {
            var infoSalud = new InformacionSalud();
            infoSalud.Observaciones = string.IsNullOrWhiteSpace(informacionSalud.observaciones) ? null : informacionSalud.observaciones;
            infoSalud.GrupoSanguineo = string.IsNullOrWhiteSpace(informacionSalud.grupo_sanguineo) ? null : informacionSalud.grupo_sanguineo;

            return infoSalud;
        }
    }
}
