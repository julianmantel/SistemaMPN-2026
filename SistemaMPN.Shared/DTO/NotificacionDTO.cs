namespace SistemaMPN.Shared.DTO
{
    public class NotificacionDto
    {
        public int Id { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime Fecha { get; set; } = DateTime.Now;
        public string Tipo { get; set; } = string.Empty;
        public bool? Leida { get; set; } = false;
    }
}
