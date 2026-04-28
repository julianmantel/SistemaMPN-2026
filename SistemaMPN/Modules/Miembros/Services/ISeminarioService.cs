using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaMPN.Modules.Miembros.Services
{
    public interface ISeminarioService
    {
        Task<bool> ActualizarEstadoActivo(int id, bool nuevoEstado);

        Task<bool> ActualizarSeminario(int id, SeminarioDTO seminarioActualizado);
    }
}