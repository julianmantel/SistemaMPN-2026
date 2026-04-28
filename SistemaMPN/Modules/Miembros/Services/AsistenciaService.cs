using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Miembros.Services;

public class AsistenciaService : IAsistenciaService
{
    private readonly DataContext _context;
    private readonly ILogger<AsistenciaService> _logger;

    public AsistenciaService(DataContext context, ILogger<AsistenciaService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<QrEventoDTO> GenerarCodigoEventoAsync(int eventoId)
    {
        var evento = await _context.Eventos.FindAsync(eventoId);
        if (evento == null)
            throw new KeyNotFoundException($"Evento con ID {eventoId} no encontrado.");

        var codigoVerificacion = GenerarCodigoUnico(evento.IdEvento);

        var qrDto = new QrEventoDTO
        {
            EventoId = evento.IdEvento,
            TituloEvento = evento.Titulo ?? "Sin título",
            CodigoVerificacion = codigoVerificacion,
            UrlCheckIn = $"/checkin?evento={eventoId}&codigo={codigoVerificacion}"
        };

        return qrDto;
    }

    public async Task<string> RegistrarAsistenciaAsync(RegistroAsistenciaDTO registro)
    {
        var partesCodigo = registro.CodigoEvento.Split('_'); 

        if (partesCodigo.Length < 3)
            return "ERROR: Código de evento inválido.";

        if (!int.TryParse(partesCodigo[0], out int eventoId))
            return "ERROR: Código de evento inválido.";

        var evento = await _context.Eventos.FindAsync(eventoId);

        if (evento == null)
            return "ERROR: Código de evento inválido.";

        var codigoVerificacion = string.Join("_", partesCodigo.Skip(1));
        var codigoEsperado = GenerarCodigoUnico(eventoId);
        
        if (codigoVerificacion != codigoEsperado)
            return "ERROR: Código de verificación inválido.";

        // Intentar encontrar miembro por DNI, luego por email
        int? miembroId = null;
        string? nombreMiembro = null;
        string? apellidoMiembro = null;

        if (!string.IsNullOrWhiteSpace(registro.Dni))
        {
            var miembroPorDni = await _context.Miembros
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Dni == registro.Dni);

            if (miembroPorDni != null)
            {
                miembroId = miembroPorDni.IdMiembros;
                nombreMiembro = miembroPorDni.Nombre;
                apellidoMiembro = miembroPorDni.Apellido;
            }
        }

        // Si no encontró por DNI, intentar por email
        if (!miembroId.HasValue && !string.IsNullOrWhiteSpace(registro.Email))
        {
            miembroId = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Correo == registro.Email && u.IdMiembros.HasValue)
                .Select(u => (int?)u.IdMiembros.Value)
                .FirstOrDefaultAsync();

            if (!miembroId.HasValue)
            {
                var miembroPorEmail = await _context.Miembros
                    .AsNoTracking()
                    .Include(m => m.Usuario)
                    .FirstOrDefaultAsync(m => m.Usuario != null && m.Usuario.Correo == registro.Email);

                if (miembroPorEmail != null)
                {
                    miembroId = miembroPorEmail.IdMiembros;
                    nombreMiembro = miembroPorEmail.Nombre;
                    apellidoMiembro = miembroPorEmail.Apellido;
                }
            }
        }

        // Verificar si ya registró asistencia
        if (miembroId.HasValue)
        {
            var yaRegistroMiembro = await _context.Asistencias
                .AnyAsync(a => a.MiembroId == miembroId && a.EventoId == evento.IdEvento);

            if (yaRegistroMiembro)
                return "DUPLICADO: Este miembro ya registró asistencia para este evento.";
        }
        else if (!string.IsNullOrWhiteSpace(registro.Email))
        {
            var yaRegistroEmail = await _context.Asistencias
                .AnyAsync(a => a.EmailVisitante == registro.Email && a.EventoId == evento.IdEvento);

            if (yaRegistroEmail)
                return "DUPLICADO: Este email ya registró asistencia para este evento.";
        }

        // Usar nombre/apellido del miembro si se encontró, sino el del formulario
        var nombreFinal = nombreMiembro ?? registro.Nombre;
        var apellidoFinal = apellidoMiembro ?? registro.Apellido;

        // Crear registro de asistencia
        var asistencia = new Asistencia
        {
            MiembroId = miembroId,
            EventoId = evento.IdEvento,
            NombreVisitante = nombreFinal,
            ApellidoVisitante = apellidoFinal,
            EmailVisitante = registro.Email,
            FechaRegistro = DateTime.UtcNow,
            CodigoVerificacion = registro.CodigoEvento,
            Origen = "QR"
        };

        _context.Asistencias.Add(asistencia);
        await _context.SaveChangesAsync();

        var tipo = miembroId.HasValue ? "miembro" : "visitante";
        return $"OK: Asistencia registrada como {tipo} - {nombreFinal} {apellidoFinal}";
    }

    public async Task<List<AsistenciaDTO>> GetAsistentesEventoAsync(int eventoId)
    {
        var evento = await _context.Eventos.FindAsync(eventoId);
        if (evento == null)
            return new List<AsistenciaDTO>();

        var asistentes = await _context.Asistencias
            .Where(a => a.EventoId == eventoId)
            .Select(a => new AsistenciaDTO
            {
                IdAsistencia = a.IdAsistencia,
                MiembroId = a.MiembroId,
                NombreMiembro = a.Miembro != null ? a.Miembro.Nombre : null,
                ApellidoMiembro = a.Miembro != null ? a.Miembro.Apellido : null,
                NombreVisitante = a.NombreVisitante,
                ApellidoVisitante = a.ApellidoVisitante,
                EmailVisitante = a.EmailVisitante,
                FechaRegistro = a.FechaRegistro,
                Origen = a.Origen,
                EventoId = a.EventoId,
                TituloEvento = evento.Titulo ?? "Sin título"
            })
            .OrderByDescending(a => a.FechaRegistro)
            .ToListAsync();

        return asistentes;
    }

    public async Task<List<AsistenciaDTO>> GetHistorialMiembroAsync(int miembroId, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _context.Asistencias
            .Where(a => a.MiembroId == miembroId)
            .AsQueryable();

        if (desde.HasValue)
            query = query.Where(a => a.FechaRegistro >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(a => a.FechaRegistro <= hasta.Value);

        var historial = await query
            .Include(a => a.Evento)
            .Select(a => new AsistenciaDTO
            {
                IdAsistencia = a.IdAsistencia,
                MiembroId = a.MiembroId,
                FechaRegistro = a.FechaRegistro,
                Origen = a.Origen,
                EventoId = a.EventoId,
                TituloEvento = a.Evento != null ? (a.Evento.Titulo ?? "Sin título") : "Evento eliminado"
            })
            .OrderByDescending(a => a.FechaRegistro)
            .ToListAsync();

        return historial;
    }

    public async Task<int> GetCantidadAsistentesAsync(int eventoId)
    {
        return await _context.Asistencias.CountAsync(a => a.EventoId == eventoId);
    }

    public async Task<bool> YaRegistroAsync(int? miembroId, string? email, int eventoId)
    {
        if (miembroId.HasValue)
        {
            return await _context.Asistencias
                .AnyAsync(a => a.MiembroId == miembroId && a.EventoId == eventoId);
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            return await _context.Asistencias
                .AnyAsync(a => a.EmailVisitante == email && a.EventoId == eventoId);
        }

        return false;
    }

    private static string GenerarCodigoUnico(int eventoId)
    {
        return $"QR_EVT_{eventoId:D6}_MPN";
    }
}
