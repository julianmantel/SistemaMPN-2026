using System.Text.Json.Serialization;

namespace SistemaMPN.Shared.DTO
{
    public class PeticionBaseDTO
    {
        public int IdPeticiones { get; set; }
        
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;
        
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaRespuesta { get; set; }

        [JsonPropertyName("estado")]
        public string Estado { get; set; } = string.Empty;
        public string? Mensaje { get; set; }
        public string? NombreSolicitante { get; set; }
        public string? NombreQuienResponde { get; set; }
    }

    public class PeticionReunionDetalleDTO : PeticionBaseDTO
    {
        [JsonPropertyName("motivo")]
        public string? Motivo { get; set; }
        
        [JsonPropertyName("fechaPreferida")]
        public DateOnly? FechaPreferida { get; set; }
        
        [JsonPropertyName("correo")]
        public string? Correo { get; set; }
        
        [JsonPropertyName("idMiembro")]
        public int? IdMiembro { get; set; }
        
        [JsonPropertyName("nombreMiembro")]
        public string? NombreMiembro { get; set; }
    }

    public class PeticionCambioDetalleDTO : PeticionBaseDTO
    {
        [JsonPropertyName("idMiembro")]
        public int? IdMiembro { get; set; }
        
        [JsonPropertyName("nombreMiembro")]
        public string? NombreMiembro { get; set; }
        
        [JsonPropertyName("idGrupo")]
        public int? IdGrupo { get; set; }
        
        [JsonPropertyName("nombreGrupo")]
        public string? NombreGrupo { get; set; }
    }

    public class PeticionActualizacionDetalleDTO : PeticionBaseDTO
    {
        [JsonPropertyName("idMiembro")]
        public int? IdMiembro { get; set; }
        
        [JsonPropertyName("nombreMiembro")]
        public string? NombreMiembro { get; set; }
    }

    public class PeticionAgregarDetalleDTO : PeticionBaseDTO
    {
        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }
        
        [JsonPropertyName("apellido")]
        public string? Apellido { get; set; }
        
        [JsonPropertyName("dni")]
        public string? Dni { get; set; }
        
        [JsonPropertyName("nacionalidad")]
        public string? Nacionalidad { get; set; }
        
        [JsonPropertyName("fechaNacimiento")]
        public DateOnly? FechaNacimiento { get; set; }
        
        [JsonPropertyName("lugarNacimiento")]
        public string? LugarNacimiento { get; set; }
        
        [JsonPropertyName("telefono")]
        public string? Telefono { get; set; }
        
        [JsonPropertyName("sexo")]
        public char? Sexo { get; set; }
    }
}
