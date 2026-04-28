using System;
using System.Collections.Generic;
namespace SistemaMPN.Shared.Models;

public partial class Usuario
{
    public int IdUsuarios { get; set; }

    public string? UserName { get; set; }

    public string? Correo { get; set; }

    public byte[]? PasswordHash { get; set; }

    public byte[]? PasswordSalt { get; set; }

    public int? IdMiembros { get; set; }

    public virtual Miembro? IdMiembrosNavigation { get; set; }

    public virtual ICollection<NotificacionUsuario> NotificacionUsuarios { get; set; } = new List<NotificacionUsuario>();

    public virtual ICollection<PeticionCambiarPassword> PeticionCambiarPasswords { get; set; } = new List<PeticionCambiarPassword>();

    public virtual Peticion? Peticion { get; set; }

    public virtual ICollection<Rol> IdRols { get; set; } = new List<Rol>();
}
