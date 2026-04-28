namespace SistemaMPN.Modules.Autentificacion.Models
{
    public class BulkEmailRequest
    {
        public List<string> Recipients { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
