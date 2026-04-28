using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaMPN.Shared.DTO
{
    public class InformacionAcademicaDTO
    {
        public int? id { get; set; }

        [MaxLength(40, ErrorMessage = "No puede superar los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string estudios_primario { get; set; } = string.Empty;

        [MaxLength(40, ErrorMessage = "No puede superar los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string estudios_secundario { get; set; } = string.Empty;

        [MaxLength(40, ErrorMessage = "No puede superar los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string estudios_terciario { get; set; } = string.Empty;

        [MaxLength(40, ErrorMessage = "No puede superar los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string estudios_universitario { get; set; } = string.Empty;

        [MaxLength(40, ErrorMessage = "No puede superar los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string carrera { get; set; } = string.Empty;

        [MaxLength(40, ErrorMessage = "No puede superar los 40 caracteres")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d|.*[A-Za-z]).*\S.*$", ErrorMessage = "Debe contener letras y no puede ser solo números o símbolos")]
        public string cursos_realizados { get; set; } = string.Empty;

        public bool tiene_informacion_academica { get; set; } = false;
    }
}
