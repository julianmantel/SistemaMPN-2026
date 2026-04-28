# SistemaMPN-2026

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/)
[![Blazor 8.0](https://img.shields.io/badge/Blazor-8.0-512BD4)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![MudBlazor](https://img.shields.io/badge/MudBlazor-8.5.1-FF7043)](https://www.mudblazor.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-14+-336791)](https://www.postgresql.org/)
[![Licencia](https://img.shields.io/badge/Licencia-MIT-green.svg)](LICENSE)

Sistema de gestión eclesiástica desarrollado con tecnologías .NET, que permite la administración integral de una organización religiosa.

## Áreas del Sistema

### Miembros
Gestión integral de miembros incluyendo registro y datos completos (personales, salud, información laboral y académica, datos eclesiásticos), organización en grupos, seguimiento de seminarios cursados, control de asistencia a eventos mediante códigos QR y sistema de peticiones para cambios y altas.

### Tesorería
Módulo financiero que incluye gestión de documentos contables, emisión de recibos (generales y ofrendas voluntarias), planilla de diezmos, registro de movimientos de caja y administración de turnos de tesorería.

### Consultoría
Administración del proceso de consultoría eclesiástica con gestión de reuniones entre consultores y miembros, registro de notas vinculadas a cada reunión y seguimiento del estados de las mismas.

### Administración
Centro de control del sistema que abarca autenticación y gestión de usuarios con perfiles jerárquicos (admin, gestor de miembros, consultor, tesorero, líder), sistema de notificaciones en tiempo real y respaldo de base de datos.

## Tecnologías y Frameworks

| Capa | Tecnología |
|------|-------------|
| Backend | .NET 8.0, ASP.NET Core Web API |
| Frontend | Blazor WebAssembly 8.0 |
| UI | MudBlazor 8.5.1 |
| Base de datos | PostgreSQL con Entity Framework Core |
| Autenticación | JWT (JSON Web Tokens) |
| Tiempo real | SignalR |
| Almacenamiento en la nube | Mega.nz API |
| Correo electrónico | MailKit / MimeKit |
| Documentos Excel | ClosedXML |
| Códigos QR | QRCoder |

## Dependencias Externas

Para el correcto funcionamiento del sistema se requieren los siguientes componentes externos:

- **PostgreSQL** - Servidor de base de datos. Puede ejecutarse de forma local o en contenedor Docker.
- **Servidor SMTP** - Servidor de correo saliente para el envío de notificaciones por email.
- **Cuenta Mega.nz** - Credenciales de acceso para el almacenamiento de archivos en la nube.
- **reCAPTCHA v2** - Configuración de claves (Site Key y Secret Key) proporcionadas por Google Cloud Console.

## Configuración

El sistema utiliza variables de configuración que deben configurarse en el archivo `appsettings.json` o mediante variables de entorno
Para un ejemplo de esto revisar el example.env en la carpeta de SistemaMPN

## Estructura del Proyecto

```
SistemaMPN-2026/
├── SistemaMPN/           # Proyecto API (Backend)
├── SistemaMPN.Client/    # Proyecto Blazor (Frontend)
├── SistemaMPN.Shared/    # Biblioteca compartida (DTOs)
└── TablasSistemaMPN.sql  # Script de base de datos
```

## Requisitos para Ejecución

- .NET 8.0 SDK
- PostgreSQL 14+
- Node.js (solo para desarrollo)
