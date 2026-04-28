using System;
using System.Collections.Generic;

namespace SistemaMPN.Shared.Models;

    public partial class Miembro
    {
        public int IdMiembros { get; set; }

        public string Dni { get; set; } = null!;

        public string Nombre { get; set; } = null!;

        public string Apellido { get; set; } = null!;

        public char Sexo { get; set; }

        public DateOnly FechaNacimiento { get; set; }

        public DateOnly? FechaHasta { get; set; }

        public string Nacionalidad { get; set; } = null!;

        public string? LugarNacimiento { get; set; }

        public string? Telefono { get; set; }

        public string? TelefonoFijo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int? IdDireccion { get; set; }

    public int? IdDatosPersonales { get; set; }

    public int? IdTrayectoria { get; set; }

    public int? IdInformacionSalud { get; set; }

    public int? IdInformacionEclesiastica { get; set; }

    public virtual Consultor? Consultore { get; set; }

    public virtual GestorMiembro? GestoresMiembro { get; set; }

    public virtual DatoPersonal? IdDatosPersonalesNavigation { get; set; }

    public virtual Direccion? IdDireccionNavigation { get; set; }

    public virtual InformacionEclesiastica? IdInformacionEclesiasticaNavigation { get; set; }

    public virtual InformacionSalud? IdInformacionSaludNavigation { get; set; }

    public virtual Trayectoria? IdTrayectoriaNavigation { get; set; }

    public virtual Lider? Lider { get; set; }

    public virtual ICollection<PerteneceGrupo> PerteneceGrupos { get; set; } = new List<PerteneceGrupo>();

    public virtual Peticion? Peticion { get; set; }

    public virtual ICollection<PeticionActualizacion> PeticionesActualizacion { get; set; } = new List<PeticionActualizacion>();

    public virtual ICollection<PeticionCambio> PeticionesCambio { get; set; } = new List<PeticionCambio>();

    public virtual ICollection<PeticionReunion> PeticionesReunion { get; set; } = new List<PeticionReunion>();

    public virtual Tesorero? Tesorero { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
