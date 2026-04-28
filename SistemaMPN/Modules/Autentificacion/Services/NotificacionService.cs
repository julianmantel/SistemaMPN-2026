using System.Linq;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Data;
using SistemaMPN.Hubs;
using SistemaMPN.Shared.DTO;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Modules.Autentificacion.Services
{
    public class NotificacionService : INotificacionService
    {
        private readonly IHubContext<NotificacionesHub> _hub;
        private readonly DataContext _context;

        public NotificacionService(IHubContext<NotificacionesHub> hub, DataContext context)
        {
            _hub = hub;
            _context = context;
        }

        public async Task EnviarNotificacionPeticion(PeticionDTO peticion)
        {
            string mensaje = "Se ha registrado una nueva petición";

            mensaje = peticion.Tipo switch
            {
                "Cambio" => "Se ha registrado una nueva petición de cambio de grupo.",
                "Actualizacion" => "Se ha registrado una nueva petición de actualización de datos.",
                "Agregar" => "Se ha registrado una nueva petición para agregar un miembro.",
                "Reunion" => "Se ha registrado una nueva petición de reunión.",
                _ => mensaje
            };

            
            var notificacion = new Notificacion
            {
                Mensaje = mensaje,
                Fecha = peticion.FechaSolicitud,
                Tipo = peticion.Tipo
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            var queryUsuarios = _context.Usuarios.AsQueryable();

            if(peticion.Tipo == "Reunion")
            {
                queryUsuarios = queryUsuarios.Where(u => u.IdRols.Any(r => r.Nombre.Equals("Consultor")));
            } else
            {
                queryUsuarios = queryUsuarios.Where(u => u.IdRols.Any(r => r.Nombre.Equals("Admin") || r.Nombre.Equals("Gestor de Miembros")));
            }

            var destinatarios = await queryUsuarios.Select(u => u.IdUsuarios).ToListAsync();
            
            var notificacionesUsuarios = destinatarios.Select(usuario =>
                new NotificacionUsuario
                {
                    IdNotificacion = notificacion.IdNotificaciones,
                    IdUsuario = usuario,
                    Leida = false
                });

            _context.NotificacionUsuario.AddRange(notificacionesUsuarios);
            await _context.SaveChangesAsync();


            await _hub.Clients.Users(destinatarios.ConvertAll<string>(u => u.ToString()))
                    .SendAsync("RecibirNotificacion", new NotificacionDto
            {
                    Id = notificacion.IdNotificaciones,
                    Mensaje = mensaje,
                    Fecha = notificacion.Fecha ?? DateTime.Now,
                    Tipo = notificacion.Tipo
                });
        }

        public async Task EnviarNotificacionPropuesta(PropuestaDeCambioDTO propuesta)
        {
            string mensaje = "Se ha registrado una nueva propuesta de cambio de turno";

            var notificacion = new Notificacion
            {
                Mensaje = mensaje,
                Fecha = propuesta.FechaSolicitud,
                Tipo = "Propuesta"
            };

            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            var destinatarios = await _context.Usuarios
                .Where(u => u.IdMiembrosNavigation.IdMiembros == propuesta.IdReceptor || u.IdRols.Any(r => r.Nombre.Equals("ProTesorero")))
                .Select(u => u.IdUsuarios)
                .ToListAsync();

            var notificacionesUsuarios = destinatarios.Select(idUsuario =>
                new NotificacionUsuario
                {
                    IdNotificacion = notificacion.IdNotificaciones,
                    IdUsuario = idUsuario,
                    Leida = false
                });

            _context.NotificacionUsuario.AddRange(notificacionesUsuarios);
            await _context.SaveChangesAsync();

            await _hub.Clients.Users(destinatarios.ConvertAll<string>(u => u.ToString())).SendAsync("RecibirNotificacion", new NotificacionDto
            {
                Id = notificacion.IdNotificaciones,
                Mensaje = mensaje,
                Fecha = propuesta.FechaSolicitud,
                Tipo = "Propuesta"
            });
        }
    }
}
