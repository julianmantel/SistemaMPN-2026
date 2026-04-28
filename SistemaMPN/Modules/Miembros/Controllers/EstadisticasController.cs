using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using SistemaMPN.Data;
using SistemaMPN.Shared.DTO;

namespace SistemaMPN.Modules.Miembros.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin, gestor de miembros, auditor")]
    public class EstadisticasController : Controller
    {
        private readonly ILogger<EstadisticasController> _logger;
        private readonly DataContext _context;

        public List<RangoEdadesDTO> RangoEdades { get; set; } = new();
        List<OcupacionGrupoDTO> OcupacionGrupos { get; set; } = new();
        List<MiembrosPorSexoDto> MiembrosPorSexo { get; set; } = new();
        List<BautismoEstadoDTO> BautismoEstados { get; set; } = new();
        List<MiembrosPorFechaCreacionDto> MiembrosPorFechaCreacion { get; set; } = new();

        public EstadisticasController(ILogger<EstadisticasController> logger, DataContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("GetEstadisticas")]
        public async Task<ActionResult<EstadisticasDTO>> GetEstadisticas()
        {
            try
            {
                await GetMiembrosPorSexo();
                await GetRangoEdades();
                await GetOcupacionGrupos();
                await GetBautismoEstado();
                await GetMiembrosPorFechaCreacion();

                var estadisticas = new EstadisticasDTO()
                {
                    MiembrosPorSexo = this.MiembrosPorSexo,
                    RangoEdades = this.RangoEdades,
                    OcupacionGrupos = this.OcupacionGrupos,
                    BautismoEstados = this.BautismoEstados,
                    MiembrosPorFechaCreacion = this.MiembrosPorFechaCreacion
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las estadísticas de miembros.");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        private async Task GetMiembrosPorSexo()
        {
            MiembrosPorSexo = await _context.Miembros
                            .GroupBy(m => m.Sexo)
                            .Select(g => new MiembrosPorSexoDto
                            {
                                Sexo = g.Key == 'M' ? "Masculino" :
                                       g.Key == 'F' ? "Femenino" :
                                                "No especificado",
                                Cantidad = g.Count()
                            }).ToListAsync();
        }

        private async Task GetRangoEdades()
        {
            var hoy = DateTime.Today;

            var data = await _context.Miembros
                .Where(m => m.FechaHasta == null)
                .Select(m => new
                {
                    Edad = hoy.Year - m.FechaNacimiento.Year
                })
                .ToListAsync();

            RangoEdades = new List<RangoEdadesDTO>
            {
                new() { Rango = "16-29", Cantidad = data.Count(x => x.Edad >= 16 && x.Edad <= 29) },
                new() { Rango = "30-44", Cantidad = data.Count(x => x.Edad >= 30 && x.Edad <= 44) },
                new() { Rango = "45-59", Cantidad = data.Count(x => x.Edad >= 45 && x.Edad <= 59) },
                new() { Rango = "60+", Cantidad = data.Count(x => x.Edad >= 60) }
            };
        }

        private async Task GetOcupacionGrupos()
        {
            OcupacionGrupos = await _context.Grupos
                          .Select(g => new OcupacionGrupoDTO
                          {
                              Grupo = g.Nombre,
                              Porcentaje = (g.PerteneceGrupos.Count() * 100.0 / g.MaxCantMiembros) ?? 1.0
                          }).ToListAsync();
        }

        private async Task GetBautismoEstado()
        {
            var bautizados = await _context.Miembros
                                   .CountAsync(m =>
                                        m.FechaHasta == null &&
                                        m.IdInformacionEclesiasticaNavigation != null &&
                                        m.IdInformacionEclesiasticaNavigation.IdBautismos != null);

            var total = await _context.Miembros
                .CountAsync(m => m.FechaHasta == null);

            BautismoEstados = new List<BautismoEstadoDTO>
            {
                new() { Estado = "Bautizados", Cantidad = bautizados },
                new() { Estado = "No bautizados", Cantidad = total - bautizados }
            };
        }

        private async Task GetMiembrosPorFechaCreacion()
        {
            MiembrosPorFechaCreacion = await _context.Miembros
                .Where(m => m.FechaCreacion != null)
                .GroupBy(m => new 
                { 
                    m.FechaCreacion.Year, 
                    m.FechaCreacion.Month, 
                    m.FechaCreacion.Day 
                })
                .Select(g => new MiembrosPorFechaCreacionDto
                {
                    Año = g.Key.Year,
                    Mes = g.Key.Month,
                    Dia = g.Key.Day,
                    Cantidad = g.Count()
                })
                .OrderBy(x => x.Año)
                .ThenBy(x => x.Mes)
                .ThenBy(x => x.Dia)
                .ToListAsync();
        }

        [HttpGet("ExportarEstadisticas")]
        public async Task<IActionResult> ExportarEstadisticas()
        {
            try
            {
                await GetMiembrosPorSexo();
                await GetRangoEdades();
                await GetOcupacionGrupos();
                await GetBautismoEstado();
                await GetMiembrosPorFechaCreacion();

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Estadisticas");

                ws.Cell(1, 1).Value = "ESTADÍSTICAS DE MIEMBROS";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Range(1, 1, 1, 3).Merge();
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(2, 1).Value = $"Fecha de generación: {DateTime.Now:dd/MM/yyyy HH:mm}";
                ws.Cell(2, 1).Style.Font.Italic = true;

                int fila = 4;

                ws.Cell(fila, 1).Value = "1. MIEMBROS POR SEXO";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Font.FontSize = 12;
                fila++;

                ws.Cell(fila, 1).Value = "Sexo";
                ws.Cell(fila, 2).Value = "Cantidad";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 2).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(fila, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                fila++;

                int totalSexo = 0;
                foreach (var item in MiembrosPorSexo)
                {
                    ws.Cell(fila, 1).Value = item.Sexo;
                    ws.Cell(fila, 2).Value = item.Cantidad;
                    totalSexo += item.Cantidad;
                    fila++;
                }
                ws.Cell(fila, 1).Value = "TOTAL";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 2).Value = totalSexo;
                ws.Cell(fila, 2).Style.Font.Bold = true;
                fila += 2;

                ws.Cell(fila, 1).Value = "2. RANGO DE EDADES";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Font.FontSize = 12;
                fila++;

                ws.Cell(fila, 1).Value = "Rango";
                ws.Cell(fila, 2).Value = "Cantidad";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 2).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(fila, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                fila++;

                int totalEdades = 0;
                foreach (var item in RangoEdades)
                {
                    ws.Cell(fila, 1).Value = item.Rango;
                    ws.Cell(fila, 2).Value = item.Cantidad;
                    totalEdades += item.Cantidad;
                    fila++;
                }
                ws.Cell(fila, 1).Value = "TOTAL";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 2).Value = totalEdades;
                ws.Cell(fila, 2).Style.Font.Bold = true;
                fila += 2;

                ws.Cell(fila, 1).Value = "3. OCUPACIÓN DE GRUPOS (%)";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Font.FontSize = 12;
                fila++;

                ws.Cell(fila, 1).Value = "Grupo";
                ws.Cell(fila, 2).Value = "Ocupación (%)";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 2).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(fila, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                fila++;

                foreach (var item in OcupacionGrupos)
                {
                    ws.Cell(fila, 1).Value = item.Grupo;
                    ws.Cell(fila, 2).Value = item.Porcentaje;
                    ws.Cell(fila, 2).Style.NumberFormat.Format = "0.00";
                    fila++;
                }
                fila += 2;

                ws.Cell(fila, 1).Value = "4. ESTADO DE BAUTISMO";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Font.FontSize = 12;
                fila++;

                ws.Cell(fila, 1).Value = "Estado";
                ws.Cell(fila, 2).Value = "Cantidad";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 2).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(fila, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                fila++;

                int totalBautismo = 0;
                foreach (var item in BautismoEstados)
                {
                    ws.Cell(fila, 1).Value = item.Estado;
                    ws.Cell(fila, 2).Value = item.Cantidad;
                    totalBautismo += item.Cantidad;
                    fila++;
                }
                ws.Cell(fila, 1).Value = "TOTAL";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 2).Value = totalBautismo;
                ws.Cell(fila, 2).Style.Font.Bold = true;
                fila += 2;

                ws.Cell(fila, 1).Value = "5. MIEMBROS POR FECHA DE CREACIÓN";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Font.FontSize = 12;
                fila++;

                ws.Cell(fila, 1).Value = "Año";
                ws.Cell(fila, 2).Value = "Mes";
                ws.Cell(fila, 3).Value = "Día";
                ws.Cell(fila, 4).Value = "Cantidad";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 2).Style.Font.Bold = true;
                ws.Cell(fila, 3).Style.Font.Bold = true;
                ws.Cell(fila, 4).Style.Font.Bold = true;
                ws.Cell(fila, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(fila, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(fila, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Cell(fila, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
                fila++;

                int totalFechas = 0;
                foreach (var item in MiembrosPorFechaCreacion)
                {
                    ws.Cell(fila, 1).Value = item.Año;
                    ws.Cell(fila, 2).Value = item.Mes;
                    ws.Cell(fila, 3).Value = item.Dia;
                    ws.Cell(fila, 4).Value = item.Cantidad;
                    totalFechas += item.Cantidad;
                    fila++;
                }
                ws.Cell(fila, 1).Value = "TOTAL";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 4).Value = totalFechas;
                ws.Cell(fila, 4).Style.Font.Bold = true;

                ws.Column(1).Width = 25;
                ws.Column(2).Width = 15;
                ws.Column(3).Width = 15;
                ws.Column(4).Width = 15;

                ws.RangeUsed().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.RangeUsed().Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"EstadisticasMiembros_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar las estadísticas de miembros.");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("GetEstadisticasAsistencia")]
        public async Task<ActionResult<object>> GetEstadisticasAsistencia()
        {
            try
            {
                var totalAsistencias = await _context.Asistencias.CountAsync();
                var miembrosConAsistencia = await _context.Asistencias
                    .Where(a => a.MiembroId.HasValue)
                    .Select(a => a.MiembroId.Value)
                    .Distinct()
                    .CountAsync();
                var visitantesUnicos = await _context.Asistencias
                    .Where(a => !a.MiembroId.HasValue && !string.IsNullOrEmpty(a.EmailVisitante))
                    .Select(a => a.EmailVisitante)
                    .Distinct()
                    .CountAsync();

                // Asistencias por evento
                var asistenciasPorEvento = await _context.Asistencias
                    .Include(a => a.Evento)
                    .GroupBy(a => new { a.EventoId, a.Evento.Titulo })
                    .Select(g => new
                    {
                        EventoId = g.Key.EventoId,
                        Titulo = g.Key.Titulo ?? "Sin título",
                        Cantidad = g.Count(),
                        MiembrosRegistrados = g.Count(a => a.MiembroId.HasValue),
                        Visitantes = g.Count(a => !a.MiembroId.HasValue)
                    })
                    .OrderByDescending(x => x.Cantidad)
                    .ToListAsync();

                // Asistencias por mes (últimos 12 meses)
                var hace12Meses = DateTime.Now.AddMonths(-12);
                var asistenciasPorMes = await _context.Asistencias
                    .Where(a => a.FechaRegistro >= hace12Meses)
                    .GroupBy(a => new { a.FechaRegistro.Year, a.FechaRegistro.Month })
                    .Select(g => new
                    {
                        Año = g.Key.Year,
                        Mes = g.Key.Month,
                        Cantidad = g.Count()
                    })
                    .OrderBy(x => x.Año)
                    .ThenBy(x => x.Mes)
                    .ToListAsync();

                var estadisticasAsistencia = new
                {
                    TotalAsistencias = totalAsistencias,
                    MiembrosConAsistencia = miembrosConAsistencia,
                    VisitantesUnicos = visitantesUnicos,
                    AsistenciasPorEvento = asistenciasPorEvento,
                    AsistenciasPorMes = asistenciasPorMes
                };

                return Ok(estadisticasAsistencia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las estadísticas de asistencia.");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("ExportarAsistenciaExcel/{eventoId:int}")]
        public async Task<IActionResult> ExportarAsistenciaExcel(int eventoId)
        {
            try
            {
                var evento = await _context.Eventos.FindAsync(eventoId);
                if (evento == null)
                    return NotFound("Evento no encontrado");

                var asistentes = await _context.Asistencias
                    .Where(a => a.EventoId == eventoId)
                    .Include(a => a.Miembro)
                    .OrderByDescending(a => a.FechaRegistro)
                    .ToListAsync();

                using var workbook = new XLWorkbook();
                var ws = workbook.Worksheets.Add("Asistentes");

                ws.Cell(1, 1).Value = $"ASISTENCIA - {evento.Titulo?.ToUpper() ?? "SIN TÍTULO"}";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;
                ws.Range(1, 1, 1, 5).Merge();
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                ws.Cell(2, 1).Value = $"Fecha del evento: {evento.Fecha?.ToString("dd/MM/yyyy") ?? "N/A"}";
                ws.Cell(3, 1).Value = $"Exportado: {DateTime.Now:dd/MM/yyyy HH:mm}";

                int fila = 5;
                ws.Cell(fila, 1).Value = "Nombre";
                ws.Cell(fila, 2).Value = "Apellido";
                ws.Cell(fila, 3).Value = "Tipo";
                ws.Cell(fila, 4).Value = "Email";
                ws.Cell(fila, 5).Value = "Fecha Registro";
                for (int col = 1; col <= 5; col++)
                {
                    ws.Cell(fila, col).Style.Font.Bold = true;
                    ws.Cell(fila, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                }
                fila++;

                foreach (var a in asistentes)
                {
                    ws.Cell(fila, 1).Value = a.Miembro != null ? a.Miembro.Nombre : a.NombreVisitante ?? "";
                    ws.Cell(fila, 2).Value = a.Miembro != null ? a.Miembro.Apellido : a.ApellidoVisitante ?? "";
                    ws.Cell(fila, 3).Value = a.MiembroId.HasValue ? "Miembro" : "Visitante";
                    ws.Cell(fila, 4).Value = a.EmailVisitante ?? "";
                    ws.Cell(fila, 5).Value = a.FechaRegistro.ToString("dd/MM/yyyy HH:mm");
                    fila++;
                }

                ws.Cell(fila, 1).Value = "TOTAL";
                ws.Cell(fila, 1).Style.Font.Bold = true;
                ws.Cell(fila, 3).Value = asistentes.Count;

                for (int col = 1; col <= 5; col++)
                {
                    ws.Column(col).Width = 25;
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;

                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Asistencia_{evento.Titulo?.Replace(' ', '_')}_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar asistencia del evento {EventoId}", eventoId);
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
