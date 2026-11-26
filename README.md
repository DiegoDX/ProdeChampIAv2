# Prode

[![CI](https://github.com/DiegoDX/ProdeChampIAv2/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/DiegoDX/ProdeChampIAv2/actions/workflows/ci.yml)

Aplicación web para pronósticos de fútbol entre amigos. Un grupo crea una liga sobre un torneo (Mundial, Copa América), cada integrante pronostica los partidos y elige campeón y goleador. Gana quien acumule más puntos.

## Reglas de puntaje
| Acierto | Puntos |
|---|---|
| Resultado exacto | 5 |
| Solo ganador o empate | 3 |
| Campeón | 10 |
| Goleador | 10 |

Los pronósticos de un partido se cierran al comenzar el partido.

## Stack
- Backend: .NET, C#, EF Core, SQL Server (carpeta `backend`)
- Frontend: Angular (carpeta `frontend`)

## Estado
En construcción. El trabajo está dividido en issues; cada issue se entrega en un pull request.

## Cómo ejecutarlo

### Ejecutar el backend

**Requisitos**
- [.NET SDK 10](https://dotnet.microsoft.com/download) (LTS), versión 10.0.401 o un parche posterior de la misma banda (10.0.4xx), fijada en `backend/global.json`
- SQL Server (local, LocalDB o en Docker)

**Estructura** (`/backend`)
- `src/Prode.Domain`: entidades y reglas puras, sin dependencias.
- `src/Prode.Application`: casos de uso e interfaces; depende solo de Domain.
- `src/Prode.Infrastructure`: EF Core (`ProdeDbContext`) e implementaciones de las interfaces.
- `src/Prode.Api`: Web API (configuración, inyección de dependencias y endpoints).
- `tests/Prode.UnitTests`: tests con xUnit.
- `tests/Prode.IntegrationTests`: tests de los endpoints con `WebApplicationFactory` y SQLite en memoria (no requieren SQL Server).

**Configurar la cadena de conexión**

La clave `ConnectionStrings:Default` está vacía en `appsettings.json`. El valor real se guarda con *user-secrets* y nunca se sube al repositorio:

```bash
cd backend
dotnet user-secrets set "ConnectionStrings:Default" "Server=localhost;Database=Prode;Trusted_Connection=True;TrustServerCertificate=True" --project src/Prode.Api
```

**Configurar la autenticación (JWT) y el administrador**

La API no arranca sin una clave de firma JWT de al menos 32 caracteres. Emisor y audiencia tienen valores por defecto en `appsettings.json` (sección `Jwt`); la clave (`Jwt:SigningKey`) está vacía y se guarda con *user-secrets*. El administrador global es el usuario que se registra con el email configurado en `Admin:Email`:

```bash
cd backend
dotnet user-secrets set "Jwt:SigningKey" "<una-clave-aleatoria-de-al-menos-32-caracteres>" --project src/Prode.Api
dotnet user-secrets set "Admin:Email" "<email-del-administrador>" --project src/Prode.Api
```

En producción se configuran con variables de entorno (`Jwt__SigningKey`, `Admin__Email`).

**Aplicar las migraciones**

Las migraciones de EF Core están en `src/Prode.Infrastructure/Persistence/Migrations`. La herramienta `dotnet-ef` se instala como herramienta local del repositorio (`backend/dotnet-tools.json`), así que no hace falta instalarla de forma global:

```bash
cd backend
dotnet tool restore
dotnet ef database update --project src/Prode.Infrastructure --startup-project src/Prode.Api
```

El comando usa la cadena de conexión `ConnectionStrings:Default` configurada antes y crea la base si no existe.

Para crear una migración nueva después de cambiar el modelo:

```bash
dotnet ef migrations add <Nombre> --project src/Prode.Infrastructure --startup-project src/Prode.Api --output-dir Persistence/Migrations
```

**Compilar, probar y ejecutar**

```bash
cd backend
dotnet build
dotnet test
dotnet run --project src/Prode.Api --launch-profile http
```

- Salud: `GET http://localhost:5231/health` responde `{ "status": "ok" }`.
- Swagger UI (solo en desarrollo): `http://localhost:5231/swagger`.
- Autenticación: `POST /auth/register`, `POST /auth/login` y `GET /auth/me` (protegido). En Swagger UI, el botón **Authorize** permite pegar el token devuelto por login o registro.

### Ejecutar el frontend
Pendiente: se completa con el issue del esqueleto de Angular.
