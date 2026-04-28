namespace SistemaMPN.Shared.Models
{
    public partial class PeticionAgregar
    {
        public int IdPeticiones { get; set; }

        public string? Dni { get; set; }

        public string Nombre { get; set; } = null!;

        public string Apellido { get; set; } = null!;

        public DateOnly? FechaNacimiento { get; set; }

        public string? Nacionalidad { get; set; }

        public string? LugarNacimiento { get; set; }

        public string? Telefono { get; set; }

        public char Sexo { get; set; } = 'M';

        public virtual Peticion IdPeticionesNavigation { get; set; } = null!;
    }
}
