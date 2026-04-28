using SistemaMPN.Modules.Autentificacion.Models;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Autentificacion.Services
{
    public interface IAuthService
    {
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        public string CreateToken(Usuario user, string[] roles);
        public Task<RecaptchaResponse> ValidateRecaptcha(string token);
    }
}
