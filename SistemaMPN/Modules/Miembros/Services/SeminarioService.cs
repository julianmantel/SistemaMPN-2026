using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaMPN.Shared.Models;
using SistemaMPN.Data;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Shared.DTO;

namespace SistemaMPN.Modules.Miembros.Services
{
    public class SeminarioService : ISeminarioService
    {
        private readonly DataContext _context;


        public async Task<bool> ActualizarEstadoActivo(int id, bool nuevoEstado)
        {
            var seminario = await _context.Seminarios.FindAsync(id);
            if (seminario == null)
                return false;

            seminario.Activo = nuevoEstado;
            _context.Seminarios.Update(seminario);

            var resultado = await _context.SaveChangesAsync();
            return resultado > 0;
        }

        public async Task<bool> ActualizarSeminario(int id, SeminarioDTO seminarioActualizado)
        {
            var existente = await _context.Seminarios.FindAsync(id);
            if (existente == null)
                return false;

            existente.Nombre = seminarioActualizado.nombre;
            existente.AnioComienzo = seminarioActualizado.anio_comienzo;
            existente.Activo = seminarioActualizado.activo;

            _context.Seminarios.Update(existente);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}