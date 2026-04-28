using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.EntityFrameworkCore;
using SistemaMPN.Client.Modules.Miembros.Pages.Notificaciones;
using SistemaMPN.Shared.Models;

namespace SistemaMPN.Data
{
    public partial class DataContext : DbContext
    {
        public DataContext()
        {
        }

        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Alergia> Alergias { get; set; }

        public virtual DbSet<Bautismo> Bautismos { get; set; }

        public virtual DbSet<PeticionCambiarPassword> CambioContrasenias { get; set; }

        public virtual DbSet<CondicionMedica> CondicionesMedicas { get; set; }

        public virtual DbSet<Consultor> Consultores { get; set; }

        public virtual DbSet<DatoPersonal> DatosPersonales { get; set; }

        public virtual DbSet<Direccion> Direcciones { get; set; }

        public virtual DbSet<Evento> Eventos { get; set; }
        
        public virtual DbSet<GestorMiembro> GestoresMiembros { get; set; }

        public virtual DbSet<Grupo> Grupos { get; set; }

        public virtual DbSet<Hijo> Hijos { get; set; }

        public virtual DbSet<InformacionEclesiastica> InformacionesEclesiasticas { get; set; }

        public virtual DbSet<InformacionSalud> InformacionesSalud { get; set; }

        public virtual DbSet<Lider> Lideres { get; set; }

        public virtual DbSet<Localizacion> Localizaciones { get; set; }

        public virtual DbSet<Medicamento> Medicamentos { get; set; }

        public virtual DbSet<Miembro> Miembros { get; set; }

        public virtual DbSet<NotificacionUsuario> NotificacionUsuario { get; set; }
        public virtual DbSet<Notificacion> Notificaciones { get; set; }

        public virtual DbSet<PerteneceGrupo> PerteneceGrupos { get; set; }

        public virtual DbSet<Peticion> Peticiones { get; set; }

        public virtual DbSet<PeticionAgregar> PeticionesAgregar { get; set; }

        public virtual DbSet<PeticionActualizacion> PeticionesActualizacion { get; set; }

        public virtual DbSet<PeticionCambio> PeticionesCambio { get; set; }

        public virtual DbSet<PeticionReunion> PeticionesReunion { get; set; }

        public virtual DbSet<PropuestaCambioTurno> PropuestaCambioTurnos { get; set; }

        public virtual DbSet<Rol> Roles { get; set; }

        public virtual DbSet<Seminario> Seminarios { get; set; }

        public virtual DbSet<TelefonoEmergencia> TelefonosEmergencias { get; set; }

        public virtual DbSet<Tesorero> Tesoreros { get; set; }

        public virtual DbSet<Trayectoria> Trayectorias { get; set; }

        public virtual DbSet<Turno> Turnos { get; set; }

        public virtual DbSet<Usuario> Usuarios { get; set; }

        public virtual DbSet<SeminariosCursado> SeminariosCursados { get; set; }

        public virtual DbSet<Documento> Documentos { get; set; }

        public virtual DbSet<DocumentoTesorero> DocumentosTesoreros { get; set; }

        public virtual DbSet<Reunion> Reuniones { get; set; }

        public virtual DbSet<Nota> Notas { get; set; }

        public virtual DbSet<Asistencia> Asistencias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Alergia>(entity =>
            {
                entity.HasKey(e => e.IdAlergia).HasName("alergias_pkey");

                entity.ToTable("alergias");

                entity.Property(e => e.IdAlergia).HasColumnName("id_alergia");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .HasColumnName("nombre");
            });

            modelBuilder.Entity<Bautismo>(entity =>
            {
                entity.HasKey(e => e.IdBautismos).HasName("bautismos_pkey");

                entity.ToTable("bautismos");

                entity.Property(e => e.IdBautismos).HasColumnName("id_bautismos");
                entity.Property(e => e.Fecha).HasColumnName("fecha");
                entity.Property(e => e.Lugar)
                    .HasMaxLength(60)
                    .HasColumnName("lugar");
                entity.Property(e => e.Pastor)
                    .HasMaxLength(100)
                    .HasColumnName("pastor");
                entity.Property(e => e.Realizo)
                    .HasColumnName("realizo");
            });

            modelBuilder.Entity<PeticionCambiarPassword>(entity =>
        {
            entity.HasKey(e => e.IdPeticionCambiarPassword).HasName("peticion_cambiar_password_pkey");

            entity.ToTable("peticion_cambiar_password");

            entity.Property(e => e.IdPeticionCambiarPassword).HasColumnName("id_peticion_cambiar_password");
            entity.Property(e => e.IdUsuarios).HasColumnName("id_usuarios");
            entity.Property(e => e.ResetToken)
                .HasMaxLength(255)
                .HasColumnName("reset_token");
            entity.Property(e => e.ResetTokenExpire)
                .HasColumnType("timestamp with time zone")
                .HasColumnName("reset_token_expire");

            entity.HasOne(d => d.IdUsuariosNavigation).WithMany(p => p.PeticionCambiarPasswords)
                .HasForeignKey(d => d.IdUsuarios)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("peticion_cambiar_password_id_usuarios_fkey");
        });

            modelBuilder.Entity<CondicionMedica>(entity =>
            {
                entity.HasKey(e => e.IdCondicionMedica).HasName("condiciones_medicas_pkey");

                entity.ToTable("condiciones_medicas");

                entity.Property(e => e.IdCondicionMedica).HasColumnName("id_condicion_medica");
                entity.Property(e => e.Condicion)
                    .HasMaxLength(100)
                    .HasColumnName("condicion");
            });

            modelBuilder.Entity<Consultor>(entity =>
            {
                entity.HasKey(e => e.IdMiembros).HasName("consultores_pkey");
                entity.ToTable("consultores");
                entity.Property(e => e.IdMiembros)
                    .ValueGeneratedNever()
                    .HasColumnName("id_miembros");

                entity.HasOne(d => d.IdMiembrosNavigation)
                    .WithOne(p => p.Consultore)
                    .HasForeignKey<Consultor>(d => d.IdMiembros)
                    .HasConstraintName("consultores_id_miembros_fkey");

                entity.HasMany(c => c.Reuniones)
                    .WithOne(r => r.Consultor)
                    .HasForeignKey(r => r.IdConsultor)
                    .HasPrincipalKey(c => c.IdMiembros)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<DatoPersonal>(entity =>
            {
                entity.HasKey(e => e.IdDatosPersonales).HasName("datos_personales_pkey");

                entity.ToTable("datos_personales");

                entity.Property(e => e.IdDatosPersonales).HasColumnName("id_datos_personales");
                entity.Property(e => e.EstadoCivil)
                    .HasMaxLength(15)
                    .HasColumnName("estado_civil");
                entity.Property(e => e.Pareja)
                    .HasMaxLength(250)
                    .HasColumnName("pareja");
                entity.Property(e => e.TelefonoAlternativo)
                    .HasMaxLength(100)
                    .HasColumnName("telefono_alternativo");
            });

            modelBuilder.Entity<Direccion>(entity =>
            {
                entity.HasKey(e => e.IdDireccion).HasName("direcciones_pkey");

                entity.ToTable("direcciones");

                entity.Property(e => e.IdDireccion).HasColumnName("id_direccion");
                entity.Property(e => e.Altura).HasColumnName("altura");
                entity.Property(e => e.Barrio)
                    .HasMaxLength(250)
                    .HasColumnName("barrio");
                entity.Property(e => e.Calle)
                    .HasMaxLength(250)
                    .HasColumnName("calle");
            });

            modelBuilder.Entity<Evento>(entity =>
            {
                entity.HasKey(e => e.IdEvento).HasName("eventos_pkey");

                entity.ToTable("eventos");

                entity.Property(e => e.IdEvento).HasColumnName("id_evento");
                entity.Property(e => e.Altura).HasColumnName("altura");
                entity.Property(e => e.Calle).HasColumnName("calle");
                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .HasColumnName("color");
                entity.Property(e => e.Fecha)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("fecha");
                entity.Property(e => e.HoraFin).HasColumnName("hora_fin");
                entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
                entity.Property(e => e.Lugar).HasColumnName("lugar");
                entity.Property(e => e.Titulo).HasColumnName("titulo");
                entity.Property(e => e.Duracion_completa).HasColumnName("duracion_completa");

                entity.HasMany(d => d.IdRols).WithMany(p => p.IdEventos)
                    .UsingEntity<Dictionary<string, object>>(
                        "EventosRol",
                        r => r.HasOne<Rol>().WithMany()
                            .HasForeignKey("IdRol")
                            .HasConstraintName("eventos_rol_id_rol_fkey"),
                        l => l.HasOne<Evento>().WithMany()
                            .HasForeignKey("IdEvento")
                            .HasConstraintName("eventos_rol_id_evento_fkey"),
                        j =>
                        {
                            j.HasKey("IdEvento", "IdRol").HasName("eventos_rol_pkey");
                            j.ToTable("eventos_rol");
                            j.IndexerProperty<int>("IdEvento").HasColumnName("id_evento");
                            j.IndexerProperty<int>("IdRol").HasColumnName("id_rol");
                        });
            });

            modelBuilder.Entity<GestorMiembro>(entity =>
            {
                entity.HasKey(e => e.IdMiembros).HasName("gestores_miembros_pkey");

                entity.ToTable("gestores_miembros");

                entity.Property(e => e.IdMiembros)
                    .ValueGeneratedNever()
                    .HasColumnName("id_miembros");

                entity.HasOne(d => d.IdMiembrosNavigation).WithOne(p => p.GestoresMiembro)
                    .HasForeignKey<GestorMiembro>(d => d.IdMiembros)
                    .HasConstraintName("gestores_miembros_id_miembros_fkey");
            });

            modelBuilder.Entity<Grupo>(entity =>
            {
                entity.HasKey(e => e.IdGrupos).HasName("grupos_pkey");

                entity.ToTable("grupos");

                entity.Property(e => e.IdGrupos).HasColumnName("id_grupos");
                entity.Property(e => e.CantidadMiembros).HasColumnName("cantidad_miembros");
                entity.Property(e => e.MaxCantMiembros).HasColumnName("max_cant_miembros");
                entity.Property(e => e.IdLocalizaciones).HasColumnName("id_localizaciones");
                entity.Property(e => e.MaxCantMiembros)
                        .HasColumnName("max_cant_miembros");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(200)
                    .HasColumnName("nombre");

                entity.HasOne(d => d.IdLocalizacionesNavigation).WithMany(p => p.Grupos)
                    .HasForeignKey(d => d.IdLocalizaciones)
                    .HasConstraintName("grupos_id_localizaciones_fkey");
            });

            modelBuilder.Entity<Hijo>(entity =>
            {
                entity.HasKey(e => e.IdHijos).HasName("hijos_pkey");

                entity.ToTable("hijos");

                entity.Property(e => e.IdHijos).HasColumnName("id_hijos");
                entity.Property(e => e.Apellido)
                    .HasMaxLength(100)
                    .HasColumnName("apellido");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .HasColumnName("nombre");

                entity.HasMany(d => d.IdDatosPersonales).WithMany(p => p.IdHijos)
                    .UsingEntity<Dictionary<string, object>>(
                        "DatosPersonalesHijo",
                        r => r.HasOne<DatoPersonal>().WithMany()
                            .HasForeignKey("IdDatosPersonales")
                            .HasConstraintName("datos_personales_hijos_id_datos_personales_fkey"),
                        l => l.HasOne<Hijo>().WithMany()
                            .HasForeignKey("IdHijos")
                            .HasConstraintName("datos_personales_hijos_id_hijos_fkey"),
                        j =>
                        {
                            j.HasKey("IdHijos", "IdDatosPersonales").HasName("datos_personales_hijos_pkey");
                            j.ToTable("datos_personales_hijos");
                            j.IndexerProperty<int>("IdHijos").HasColumnName("id_hijos");
                            j.IndexerProperty<int>("IdDatosPersonales").HasColumnName("id_datos_personales");
                        });
            });

            modelBuilder.Entity<InformacionEclesiastica>(entity =>
            {
                entity.HasKey(e => e.IdInformacionEclesiastica).HasName("informaciones_eclesiasticas_pkey");

                entity.ToTable("informaciones_eclesiasticas");

                entity.Property(e => e.IdInformacionEclesiastica).HasColumnName("id_informacion_eclesiastica");
                entity.Property(e => e.Convocante)
                    .HasMaxLength(75)
                    .HasColumnName("convocante");
                entity.Property(e => e.FechaAsiste).HasColumnName("fecha_asiste");
                entity.Property(e => e.IdBautismos).HasColumnName("id_bautismos");

                entity.HasOne(d => d.IdBautismoNavigation).WithMany(p => p.InformacionesEclesiasticas)
                    .HasForeignKey(d => d.IdBautismos)
                    .HasConstraintName("informaciones_eclesiasticas_id_bautismos_fkey");
            });

            modelBuilder.Entity<InformacionSalud>(entity =>
            {
                entity.HasKey(e => e.IdInformacionSalud).HasName("informaciones_salud_pkey");

                entity.ToTable("informaciones_salud");

                entity.Property(e => e.IdInformacionSalud).HasColumnName("id_informacion_salud");
                entity.Property(e => e.GrupoSanguineo)
                    .HasMaxLength(10)
                    .HasColumnName("grupo_sanguineo");
                entity.Property(e => e.Observaciones)
                    .HasMaxLength(255)
                    .HasColumnName("observaciones");

                entity.HasMany(d => d.IdAlergia).WithMany(p => p.IdInformacionSalud)
                    .UsingEntity<Dictionary<string, object>>(
                        "SaludAlergia",
                        r => r.HasOne<Alergia>().WithMany()
                            .HasForeignKey("IdAlergia")
                            .HasConstraintName("salud_alergias_id_alergia_fkey"),
                        l => l.HasOne<InformacionSalud>().WithMany()
                            .HasForeignKey("IdInformacionSalud")
                            .HasConstraintName("salud_alergias_id_informacion_salud_fkey"),
                        j =>
                        {
                            j.HasKey("IdInformacionSalud", "IdAlergia").HasName("salud_alergias_pkey");
                            j.ToTable("salud_alergias");
                            j.IndexerProperty<int>("IdInformacionSalud").HasColumnName("id_informacion_salud");
                            j.IndexerProperty<int>("IdAlergia").HasColumnName("id_alergia");
                        });

                entity.HasMany(d => d.IdCondicionMedicas).WithMany(p => p.IdInformacionSalud)
                    .UsingEntity<Dictionary<string, object>>(
                        "SaludCondicione",
                        r => r.HasOne<CondicionMedica>().WithMany()
                            .HasForeignKey("IdCondicionMedica")
                            .HasConstraintName("salud_condiciones_id_condicion_medica_fkey"),
                        l => l.HasOne<InformacionSalud>().WithMany()
                            .HasForeignKey("IdInformacionSalud")
                            .HasConstraintName("salud_condiciones_id_informacion_salud_fkey"),
                        j =>
                        {
                            j.HasKey("IdInformacionSalud", "IdCondicionMedica").HasName("salud_condiciones_pkey");
                            j.ToTable("salud_condiciones");
                            j.IndexerProperty<int>("IdInformacionSalud").HasColumnName("id_informacion_salud");
                            j.IndexerProperty<int>("IdCondicionMedica").HasColumnName("id_condicion_medica");
                        });

                entity.HasMany(d => d.IdMedicamentos).WithMany(p => p.IdInformacionSalud)
                    .UsingEntity<Dictionary<string, object>>(
                        "SaludMedicamento",
                        r => r.HasOne<Medicamento>().WithMany()
                            .HasForeignKey("IdMedicamento")
                            .HasConstraintName("salud_medicamentos_id_medicamento_fkey"),
                        l => l.HasOne<InformacionSalud>().WithMany()
                            .HasForeignKey("IdInformacionSalud")
                            .HasConstraintName("salud_medicamentos_id_informacion_salud_fkey"),
                        j =>
                        {
                            j.HasKey("IdInformacionSalud", "IdMedicamento").HasName("salud_medicamentos_pkey");
                            j.ToTable("salud_medicamentos");
                            j.IndexerProperty<int>("IdInformacionSalud").HasColumnName("id_informacion_salud");
                            j.IndexerProperty<int>("IdMedicamento").HasColumnName("id_medicamento");
                        });
            });

            modelBuilder.Entity<Lider>(entity =>
            {
                entity.HasKey(e => e.IdMiembros).HasName("lideres_pkey");

                entity.ToTable("lideres");

                entity.Property(e => e.IdMiembros)
                    .ValueGeneratedNever()
                    .HasColumnName("id_miembros");
                entity.Property(e => e.Tipo)
                    .HasMaxLength(30)
                    .HasColumnName("tipo");

                entity.HasOne(d => d.IdMiembrosNavigation).WithOne(p => p.Lider)
                    .HasForeignKey<Lider>(d => d.IdMiembros)
                    .HasConstraintName("lideres_id_miembros_fkey");
            });

            modelBuilder.Entity<Localizacion>(entity =>
            {
                entity.HasKey(e => e.IdLocalizaciones).HasName("localizaciones_pkey");

                entity.ToTable("localizaciones");

                entity.Property(e => e.IdLocalizaciones).HasColumnName("id_localizaciones");
                entity.Property(e => e.Tipo).HasColumnName("tipo");
                entity.Property(e => e.Direccion).HasColumnName("direccion");
                entity.Property(e => e.Ubicacion).HasColumnName("ubicacion");
            });

            modelBuilder.Entity<Medicamento>(entity =>
            {
                entity.HasKey(e => e.IdMedicamento).HasName("medicamentos_pkey");

                entity.ToTable("medicamentos");

                entity.Property(e => e.IdMedicamento).HasColumnName("id_medicamento");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .HasColumnName("nombre");
            });

            modelBuilder.Entity<Miembro>(entity =>
            {
                entity.HasKey(e => e.IdMiembros).HasName("miembros_pkey");

                entity.ToTable("miembros");

                entity.HasIndex(e => e.IdDatosPersonales, "miembros_id_datos_personales_key").IsUnique();

                entity.HasIndex(e => e.IdDireccion, "miembros_id_direccion_key").IsUnique();

                entity.HasIndex(e => e.IdInformacionEclesiastica, "miembros_id_informacion_eclesiastica_key").IsUnique();

                entity.HasIndex(e => e.IdInformacionSalud, "miembros_id_informacion_salud_key").IsUnique();

                entity.HasIndex(e => e.IdTrayectoria, "miembros_id_trayectoria_key").IsUnique();

                entity.HasIndex(e => e.Dni, "miembros_dni_unique").IsUnique();

                entity.Property(e => e.IdMiembros).HasColumnName("id_miembros");
                entity.Property(e => e.Apellido)
                    .HasMaxLength(100)
                    .HasColumnName("apellido");
                entity.Property(e => e.Dni)
                    .HasMaxLength(15)
                    .HasColumnName("dni");
                entity.Property(e => e.FechaHasta).HasColumnName("fecha_hasta");
                entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
                entity.Property(e => e.IdDatosPersonales).HasColumnName("id_datos_personales");
                entity.Property(e => e.IdDireccion).HasColumnName("id_direccion");
                entity.Property(e => e.IdInformacionEclesiastica).HasColumnName("id_informacion_eclesiastica");
                entity.Property(e => e.IdInformacionSalud).HasColumnName("id_informacion_salud");
                entity.Property(e => e.IdTrayectoria).HasColumnName("id_trayectoria");
                entity.Property(e => e.LugarNacimiento)
                    .HasMaxLength(100)
                    .HasColumnName("lugar_nacimiento");
                entity.Property(e => e.Nacionalidad)
                    .HasMaxLength(150)
                    .HasColumnName("nacionalidad");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .HasColumnName("nombre");
                entity.Property(e => e.Telefono)
                    .HasMaxLength(20)
                    .HasColumnName("telefono");
                entity.Property(e => e.TelefonoFijo)
                    .HasMaxLength(20)
                    .HasColumnName("telefono_fijo");
                entity.Property(e => e.Sexo)
                    .HasMaxLength(1)
                    .HasColumnName("sexo");
                entity.Property(e => e.FechaCreacion)
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("fecha_creacion")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.IdDatosPersonalesNavigation).WithOne(p => p.Miembro)
                    .HasForeignKey<Miembro>(d => d.IdDatosPersonales)
                    .HasConstraintName("miembros_id_datos_personales_fkey");

                entity.HasOne(d => d.IdDireccionNavigation).WithOne(p => p.Miembro)
                    .HasForeignKey<Miembro>(d => d.IdDireccion)
                    .HasConstraintName("miembros_id_direccion_fkey");

                entity.HasOne(d => d.IdInformacionEclesiasticaNavigation).WithOne(p => p.Miembro)
                    .HasForeignKey<Miembro>(d => d.IdInformacionEclesiastica)
                    .HasConstraintName("miembros_id_informacion_eclesiastica_fkey");

                entity.HasOne(d => d.IdInformacionSaludNavigation).WithOne(p => p.Miembro)
                    .HasForeignKey<Miembro>(d => d.IdInformacionSalud)
                    .HasConstraintName("miembros_id_informacion_salud_fkey");

                entity.HasOne(d => d.IdTrayectoriaNavigation).WithOne(p => p.Miembro)
                    .HasForeignKey<Miembro>(d => d.IdTrayectoria)
                    .HasConstraintName("miembros_id_trayectoria_fkey");
            });

            modelBuilder.Entity<NotificacionUsuario>(entity =>
            {
                entity.HasKey(e => new { e.IdNotificacion, e.IdUsuario }).HasName("notificacion_usuario_pkey");

                entity.ToTable("notificacion_usuario");

                entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.Leida)
                    .HasDefaultValue(false)
                    .HasColumnName("leida");

                entity.HasOne(d => d.IdNotificacionNavigation).WithMany(p => p.NotificacionUsuarios)
                    .HasForeignKey(d => d.IdNotificacion)
                    .HasConstraintName("notificacion_usuario_id_notificacion_fkey");

                entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.NotificacionUsuarios)
                    .HasForeignKey(d => d.IdUsuario)
                    .HasConstraintName("notificacion_usuario_id_usuario_fkey");
            });

            modelBuilder.Entity<Notificacion>(entity =>
            {
                entity.HasKey(e => e.IdNotificaciones).HasName("notificaciones_pkey");

                entity.ToTable("notificaciones");

                entity.Property(e => e.IdNotificaciones).HasColumnName("id_notificaciones");
                entity.Property(e => e.Fecha).HasColumnName("fecha");
                entity.Property(e => e.Mensaje).HasColumnName("mensaje");
                entity.Property(e => e.Tipo)
                .HasMaxLength(60)
                .HasColumnName("tipo");
            });

            modelBuilder.Entity<PerteneceGrupo>(entity =>
            {
                entity.HasKey(e => new { e.IdGrupos, e.IdMiembros }).HasName("pertenece_grupo_pkey");

                entity.ToTable("pertenece_grupo");

                entity.Property(e => e.IdGrupos).HasColumnName("id_grupos");
                entity.Property(e => e.IdMiembros).HasColumnName("id_miembros");
                entity.Property(e => e.FechaDesde).HasColumnName("fecha_desde");
                entity.Property(e => e.Ocupacion)
                    .HasMaxLength(60)
                    .HasColumnName("ocupacion");

                entity.HasOne(d => d.IdGruposNavigation).WithMany(p => p.PerteneceGrupos)
                    .HasForeignKey(d => d.IdGrupos)
                    .HasConstraintName("pertenece_grupo_id_grupos_fkey");

                entity.HasOne(d => d.IdMiembrosNavigation).WithMany(p => p.PerteneceGrupos)
                    .HasForeignKey(d => d.IdMiembros)
                    .HasConstraintName("pertenece_grupo_id_miembros_fkey");
            });

            modelBuilder.Entity<Peticion>(entity =>
            {
                entity.HasKey(e => e.IdPeticiones).HasName("peticiones_cambio_pkey");

                entity.ToTable("peticiones");

                entity.HasIndex(e => e.IdUsuario, "fk_peticiones_usuario").IsUnique();

                entity.HasIndex(e => e.IdLider, "peticiones_cambio_id_lider_key").IsUnique();

                entity.Property(e => e.IdPeticiones)
                    .HasDefaultValueSql("nextval('peticiones_cambio_id_peticion_cambio_seq'::regclass)")
                    .HasColumnName("id_peticiones");
                entity.Property(e => e.Estado)
                    .HasMaxLength(30)
                    .HasDefaultValueSql("'En proceso'::character varying")
                    .HasColumnName("estado");
                entity.Property(e => e.FechaRespuesta).HasColumnName("fecha_respuesta");
                entity.Property(e => e.FechaSolicitud).HasColumnName("fecha_solicitud");
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.IdLider).HasColumnName("id_lider");
                entity.Property(e => e.Mensaje).HasColumnName("mensaje");
                entity.Property(e => e.Tipo)
                    .HasMaxLength(40)
                    .HasColumnName("tipo");

                entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Peticion)
                    .HasForeignKey<Peticion>(d => d.IdUsuario)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_peticiones_usuario");

                entity.HasOne(d => d.IdLiderNavigation).WithOne(p => p.Peticion)
                    .HasForeignKey<Peticion>(d => d.IdLider)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("peticiones_cambio_id_lider_fkey");
            });

            modelBuilder.Entity<PeticionActualizacion>(entity =>
            {
                entity.HasKey(e => e.IdPeticiones).HasName("peticiones_actualizacion_pkey");

                entity.ToTable("peticiones_actualizacion");

                entity.Property(e => e.IdPeticiones)
                    .ValueGeneratedNever()
                    .HasColumnName("id_peticiones");
                entity.Property(e => e.IdMiembros).HasColumnName("id_miembros");

                entity.HasOne(d => d.IdMiembrosNavigation).WithMany(p => p.PeticionesActualizacion)
                    .HasForeignKey(d => d.IdMiembros)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("peticiones_actualizacion_id_miembros_fkey");

                entity.HasOne(d => d.IdPeticionesNavigation).WithOne(p => p.PeticionActualizacion)
                    .HasForeignKey<PeticionActualizacion>(d => d.IdPeticiones)
                    .HasConstraintName("peticiones_actualizacion_id_peticiones_fkey");
            });

            modelBuilder.Entity<PeticionCambio>(entity =>
            {
                entity.HasKey(e => e.IdPeticiones).HasName("peticiones_cambio_pkey1");

                entity.ToTable("peticiones_cambio");

                entity.Property(e => e.IdPeticiones)
                    .ValueGeneratedNever()
                    .HasColumnName("id_peticiones");
                entity.Property(e => e.IdGrupos).HasColumnName("id_grupos");
                entity.Property(e => e.IdMiembros).HasColumnName("id_miembros");

                entity.HasOne(d => d.IdGruposNavigation).WithMany(p => p.PeticionesCambio)
                    .HasForeignKey(d => d.IdGrupos)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("peticiones_cambio_id_grupos_fkey");

                entity.HasOne(d => d.IdMiembrosNavigation).WithMany(p => p.PeticionesCambio)
                    .HasForeignKey(d => d.IdMiembros)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("peticiones_cambio_id_miembros_fkey");

                entity.HasOne(d => d.IdPeticionesNavigation).WithOne(p => p.PeticionCambio)
                    .HasForeignKey<PeticionCambio>(d => d.IdPeticiones)
                    .HasConstraintName("peticiones_cambio_id_peticiones_fkey");
            });

            modelBuilder.Entity<PeticionReunion>(entity =>
            {
                entity.HasKey(e => e.IdPeticiones).HasName("peticiones_reunion_pkey");

                entity.ToTable("peticiones_reunion");

                entity.Property(e => e.IdPeticiones)
                    .ValueGeneratedNever()
                    .HasColumnName("id_peticiones");
                entity.Property(e => e.Correo).HasColumnName("correo");
                entity.Property(e => e.FechaPreferida).HasColumnName("fecha_preferida");
                entity.Property(e => e.IdMiembros).HasColumnName("id_miembros");
                entity.Property(e => e.Motivo).HasColumnName("motivo");

                entity.HasOne(d => d.IdMiembrosNavigation).WithMany(p => p.PeticionesReunion)
                    .HasForeignKey(d => d.IdMiembros)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("peticiones_reunion_id_miembros_fkey");

                entity.HasOne(d => d.IdPeticionesNavigation).WithOne(p => p.PeticionReunion)
                    .HasForeignKey<PeticionReunion>(d => d.IdPeticiones)
                    .HasConstraintName("peticiones_reunion_id_peticiones_fkey");
            });

            modelBuilder.Entity<PeticionAgregar>(entity =>
            {
                entity.HasKey(e => e.IdPeticiones).HasName("peticiones_agregar_pkey");

                entity.ToTable("peticiones_agregar");

                entity.Property(e => e.IdPeticiones)
                    .ValueGeneratedNever()
                    .HasColumnName("id_peticiones");
                entity.Property(e => e.Apellido)
                    .HasMaxLength(100)
                    .HasColumnName("apellido");
                entity.Property(e => e.Dni)
                    .HasMaxLength(15)
                    .HasColumnName("dni");
                entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento");
                entity.Property(e => e.LugarNacimiento)
                    .HasMaxLength(100)
                    .HasColumnName("lugar_nacimiento");
                entity.Property(e => e.Nacionalidad)
                    .HasMaxLength(150)
                    .HasColumnName("nacionalidad");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .HasColumnName("nombre");
                entity.Property(e => e.Sexo)
                    .HasMaxLength(1)
                    .HasColumnName("sexo");
                entity.Property(e => e.Telefono)
                    .HasMaxLength(20)
                    .HasColumnName("telefono");

                entity.HasOne(d => d.IdPeticionesNavigation).WithOne(p => p.PeticionAgregar)
                    .HasForeignKey<PeticionAgregar>(d => d.IdPeticiones)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("peticiones_agregar_id_peticiones_fkey");
            });

            modelBuilder.Entity<PropuestaCambioTurno>(entity =>
            {
                entity.HasKey(e => e.IdPropuestaCambioTurno).HasName("propuesta_cambio_turno_pkey");

                entity.ToTable("propuesta_cambio_turno");

                entity.Property(e => e.IdPropuestaCambioTurno).HasColumnName("id_propuesta_cambio_turno");
                entity.Property(e => e.Estado)
                    .HasMaxLength(30)
                    .HasDefaultValueSql("'Pendiente'::character varying")
                    .HasColumnName("estado");
                entity.Property(e => e.FechaSolicitud)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("fecha_solicitud");
                entity.Property(e => e.FechaRespuesta).HasColumnName("fecha_respuesta");
                entity.Property(e => e.IdReceptor).HasColumnName("id_receptor");
                entity.Property(e => e.IdTurno).HasColumnName("id_turno");

                entity.HasOne(d => d.IdReceptorNavigation).WithMany(p => p.PropuestaCambioTurnos)
                    .HasForeignKey(d => d.IdReceptor)
                    .HasConstraintName("propuesta_cambio_turno_id_receptor_fkey");

                entity.HasOne(d => d.IdTurnoNavigation).WithMany(p => p.PropuestaCambioTurnos)
                    .HasForeignKey(d => d.IdTurno)
                    .HasConstraintName("propuesta_cambio_turno_id_turno_fkey");
            });

            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.IdRol).HasName("roles_pkey");

                entity.ToTable("roles");

                entity.Property(e => e.IdRol).HasColumnName("id_rol");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(80)
                    .HasDefaultValueSql("'Usuario'::character varying")
                    .HasColumnName("nombre");

                entity.HasMany(d => d.IdUsuarios).WithMany(p => p.IdRols)
                    .UsingEntity<Dictionary<string, object>>(
                        "RolesUsuario",
                        r => r.HasOne<Usuario>().WithMany()
                            .HasForeignKey("IdUsuarios")
                            .HasConstraintName("roles_usuarios_id_usuarios_fkey"),
                        l => l.HasOne<Rol>().WithMany()
                            .HasForeignKey("IdRol")
                            .HasConstraintName("roles_usuarios_id_rol_fkey"),
                        j =>
                        {
                            j.HasKey("IdRol", "IdUsuarios").HasName("roles_usuarios_pkey");
                            j.ToTable("roles_usuarios");
                            j.IndexerProperty<int>("IdRol").HasColumnName("id_rol");
                            j.IndexerProperty<int>("IdUsuarios").HasColumnName("id_usuarios");
                        });
            });

            modelBuilder.Entity<Seminario>(entity =>
            {
                entity.HasKey(e => e.IdSeminario).HasName("seminarios_pkey");

                entity.ToTable("seminarios");

                entity.Property(e => e.IdSeminario).HasColumnName("id_seminarios");
                entity.Property(e => e.Activo).HasColumnName("activo");
                entity.Property(e => e.AnioComienzo).HasColumnName("anio_comienzo");
                entity.Property(e => e.Nombre)
                    .HasMaxLength(150)
                    .HasColumnName("nombre");
            });

            modelBuilder.Entity<SeminariosCursado>(entity =>
            {
                entity.HasKey(e => new { e.IdSeminario, e.IdInformacionEclesiastica }).HasName("seminarios_cursados_pkey");

                entity.ToTable("seminarios_cursados");

                entity.Property(e => e.IdSeminario).HasColumnName("id_seminarios");
                entity.Property(e => e.IdInformacionEclesiastica).HasColumnName("id_informacion_eclesiastica");
                entity.Property(e => e.AnioCursado).HasColumnName("anio_cursado");
                entity.Property(e => e.Estado)
                    .HasMaxLength(20)
                    .HasColumnName("estado");

                entity.HasOne(d => d.IdInformacionEclesiasticaNavigation).WithMany(p => p.SeminariosCursados)
                    .HasForeignKey(d => d.IdInformacionEclesiastica)
                    .HasConstraintName("seminarios_cursados_id_informacion_eclesiastica_fkey");

                entity.HasOne(d => d.IdSeminariosNavigation).WithMany(p => p.SeminariosCursados)
                    .HasForeignKey(d => d.IdSeminario)
                    .HasConstraintName("seminarios_cursados_id_seminarios_fkey");
            });
            modelBuilder.Entity<DocumentoTesorero>(entity =>
            {
                entity.HasKey(e => new {e.IdTesorero, e.IdDocumento}).HasName("documentos_tesoreros_pkey");
                
                entity.ToTable("documentos_tesoreros");

                entity.Property(e => e.IdTesorero).HasColumnName("id_tesorero");
                entity.Property(e => e.IdDocumento).HasColumnName("id_documentos");
                entity.Property(e => e.EsCreador).HasColumnName("es_creador");
                entity.Property(e => e.EsSupervisor).HasColumnName("es_supervisor");
                entity.HasOne(d => d.IdDocumentoNavigation).WithMany(p => p.DocumentoTesoreros)
                    .HasForeignKey(d => d.IdDocumento)
                    .HasConstraintName("documentos_tesoreros_id_documentos_fkey");
                entity.HasOne(d => d.IdTesoreroNavigation).WithMany(p => p.DocumentoTesoreros)
                    .HasForeignKey(d => d.IdTesorero)
                    .HasConstraintName("documentos_tesoreros_id_tesorero_fkey");
            });
            modelBuilder.Entity<TelefonoEmergencia>(entity =>
            {
                entity.HasKey(e => e.IdTelefonosEmergencia).HasName("telefonos_emergencias_pkey");

                entity.ToTable("telefonos_emergencias");

                entity.Property(e => e.IdTelefonosEmergencia).HasColumnName("id_telefonos_emergencia");
                entity.Property(e => e.IdInformacionSalud).HasColumnName("id_informacion_salud");
                entity.Property(e => e.Propietario)
                    .HasMaxLength(250)
                    .HasColumnName("propietario");
                entity.Property(e => e.Telefono)
                    .HasMaxLength(20)
                    .HasColumnName("telefono");

                entity.HasOne(d => d.IdInformacionSaludNavigation).WithMany(p => p.TelefonosEmergencia)
                    .HasForeignKey(d => d.IdInformacionSalud)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("telefonos_emergencias_id_informacion_salud_fkey");
            });

            modelBuilder.Entity<Tesorero>(entity =>
            {
                entity.HasKey(e => e.IdMiembros).HasName("tesoreros_pkey");

                entity.ToTable("tesoreros");

                entity.Property(e => e.IdMiembros)
                    .ValueGeneratedNever()
                    .HasColumnName("id_miembros");
                entity.Property(e => e.IsPro)
                    .HasDefaultValue(false)
                    .HasColumnName("is_pro");

                entity.HasOne(d => d.IdMiembrosNavigation).WithOne(p => p.Tesorero)
                    .HasForeignKey<Tesorero>(d => d.IdMiembros)
                    .HasConstraintName("tesoreros_id_miembros_fkey");
            });

            modelBuilder.Entity<Trayectoria>(entity =>
            {
                entity.HasKey(e => e.IdTrayectoria).HasName("trayectorias_pkey");

                entity.ToTable("trayectorias");

                entity.Property(e => e.IdTrayectoria).HasColumnName("id_trayectoria");
                entity.Property(e => e.Carrera)
                    .HasMaxLength(200)
                    .HasColumnName("carrera");
                entity.Property(e => e.CursosRealizados)
                    .HasMaxLength(255)
                    .HasColumnName("cursos_realizados");
                entity.Property(e => e.EstudiosPrimario)
                    .HasMaxLength(20)
                    .HasColumnName("estudios_primario");
                entity.Property(e => e.EstudiosSecundario)
                    .HasMaxLength(20)
                    .HasColumnName("estudios_secundario");
                entity.Property(e => e.EstudiosTerciario)
                    .HasMaxLength(20)
                    .HasColumnName("estudios_terciario");
                entity.Property(e => e.EstudiosUniversitario)
                    .HasMaxLength(20)
                    .HasColumnName("estudios_universitario");
                entity.Property(e => e.Rubro)
                    .HasMaxLength(100)
                    .HasColumnName("rubro");
                entity.Property(e => e.SituacionLaboral)
                    .HasMaxLength(60)
                    .HasColumnName("situacion_laboral");
            });

            modelBuilder.Entity<Turno>(entity =>
            {
                entity.HasKey(e => e.IdTurnos).HasName("turnos_pkey");

                entity.ToTable("turnos");

                entity.Property(e => e.IdTurnos).HasColumnName("id_turnos");
                entity.Property(e => e.Color)
                    .HasMaxLength(10)
                    .HasColumnName("color");
                entity.Property(e => e.Fecha)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("fecha");
                entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
                entity.Property(e => e.IdTesorero).HasColumnName("id_tesorero");

                entity.HasOne(d => d.IdTesoreroNavigation).WithMany(p => p.Turnos)
                    .HasForeignKey(d => d.IdTesorero)
                    .HasConstraintName("turnos_id_tesorero_fkey");
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.IdUsuarios).HasName("usuarios_pkey");

                entity.ToTable("usuarios");

                entity.HasIndex(e => e.IdMiembros, "usuarios_id_miembros_key").IsUnique();

                entity.Property(e => e.IdUsuarios).HasColumnName("id_usuarios");
                entity.Property(e => e.Correo)
                    .HasMaxLength(150)
                    .HasColumnName("correo");
                entity.Property(e => e.IdMiembros).HasColumnName("id_miembros");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.PasswordSalt).HasColumnName("password_salt");
                entity.Property(e => e.UserName)
                    .HasMaxLength(100)
                    .HasColumnName("user_name");

                entity.HasOne(d => d.IdMiembrosNavigation).WithOne(p => p.Usuario)
                    .HasForeignKey<Usuario>(d => d.IdMiembros)
                    .HasConstraintName("usuarios_id_miembros_fkey");
            });

            modelBuilder.Entity<Documento>(entity =>
            {
                entity.HasKey(e => e.IdDocumento).HasName("documentos_pkey");

                entity.ToTable("documentos");

                entity.Property(e => e.IdDocumento).HasColumnName("id_documentos");
                entity.Property(e => e.Fecha).HasColumnName("fecha");
                entity.Property(e => e.NroDocumento).HasColumnName("nro");
                entity.Property(e => e.Tipo).HasColumnName("tipo");
                entity.Property(e => e.Firmado).HasDefaultValue(false).HasColumnName("firmado");
                entity.Property(e => e.NombreArchivoOriginal).HasDefaultValue(false).HasColumnName("nombre_archivo_original");
                entity.Property(e => e.NombreArchivoFirmado).HasDefaultValue(false).HasColumnName("nombre_archivo_firmado");
            });

            modelBuilder.Entity<Reunion>(entity =>
            {
                entity.ToTable("reuniones");
                entity.HasKey(e => e.IdReunion);
                entity.Property(e => e.IdReunion).HasColumnName("id_reunion");
                entity.Property(e => e.Fecha).HasColumnName("fecha").IsRequired();
                entity.Property(e => e.Motivo).HasColumnName("motivo").HasColumnType("TEXT");
                entity.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(200);
                entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(100);
                entity.Property(e => e.IdMiembro).HasColumnName("id_miembro").IsRequired();
                entity.Property(e => e.IdConsultor).HasColumnName("id_consultor").IsRequired();
                entity.Property(e => e.IdPeticion).HasColumnName("id_peticion");

                // Relaciones
                entity.HasOne(e => e.Miembro)
                    .WithMany()
                    .HasForeignKey(e => e.IdMiembro)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Consultor)
                    .WithMany(c => c.Reuniones)  
                    .HasForeignKey(e => e.IdConsultor)
                    .HasPrincipalKey(c => c.IdMiembros)  
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Notas)
                    .WithOne(n => n.Reunion)
                    .HasForeignKey(n => n.IdReunion)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Nota>(entity =>
            {
                entity.ToTable("notas");
                entity.HasKey(e => e.IdNota);

                entity.Property(e => e.IdNota)
                    .HasColumnName("id_nota");

                entity.Property(e => e.Comentarios)
                    .HasColumnName("comentarios")
                    .HasColumnType("TEXT");

                entity.Property(e => e.IdReunion)
                    .HasColumnName("id_reunion")
                    .IsRequired();

                // Relación
                entity.HasOne(e => e.Reunion)
                    .WithMany(r => r.Notas)
                    .HasForeignKey(e => e.IdReunion)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Asistencia>(entity =>
            {
                entity.ToTable("asistencias");
                entity.HasKey(e => e.IdAsistencia);
                entity.Property(e => e.IdAsistencia).HasColumnName("id_asistencia");

                entity.Property(e => e.MiembroId).HasColumnName("id_miembro");
                entity.Property(e => e.EventoId).HasColumnName("id_evento").IsRequired();
                entity.Property(e => e.NombreVisitante).HasColumnName("nombre_visitante").HasMaxLength(100);
                entity.Property(e => e.ApellidoVisitante).HasColumnName("apellido_visitante").HasMaxLength(100);
                entity.Property(e => e.EmailVisitante).HasColumnName("email_visitante").HasMaxLength(150);
                entity.Property(e => e.FechaRegistro).HasColumnName("fecha_registro").IsRequired();
                entity.Property(e => e.CodigoVerificacion).HasColumnName("codigo_verificacion").HasMaxLength(50);
                entity.Property(e => e.Origen).HasColumnName("origen").HasMaxLength(20).HasDefaultValue("QR");

                // Relaciones
                entity.HasOne(e => e.Evento)
                    .WithMany()
                    .HasForeignKey(e => e.EventoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Miembro)
                    .WithMany()
                    .HasForeignKey(e => e.MiembroId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Índice para prevenir duplicados
                entity.HasIndex(e => new { e.MiembroId, e.EventoId })
                    .HasDatabaseName("ix_asistencias_miembro_evento");

                entity.HasIndex(e => e.CodigoVerificacion)
                    .HasDatabaseName("ix_asistencias_codigo_verificacion");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}