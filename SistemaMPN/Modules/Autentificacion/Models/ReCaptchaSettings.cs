namespace SistemaMPN.Modules.Autentificacion.Models
{
    public class ReCaptchaSettings
    {
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
        public string VerificationUrl { get; set; }
    }
}
