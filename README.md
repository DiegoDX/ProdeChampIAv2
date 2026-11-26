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
- Torneos (cualquier usuario autenticado): `GET /tournaments` lista los torneos y `GET /tournaments/{id}/matches` devuelve los partidos ordenados por fecha, con filtro opcional `?status=Scheduled` o `?status=Finished`.

**Importar un torneo**

Solo el administrador (el usuario registrado con el email de `Admin:Email`) puede cargar torneos. La carga se hace enviando un archivo JSON a `POST /admin/tournaments/import`. En `backend/samples/sample-tournament.json` hay un ejemplo con 4 equipos y 6 partidos ficticios:

```json
{
  "name": "Nombre del torneo",
  "teams": [
    { "name": "Equipo A", "code": "EQA" }
  ],
  "matches": [
    { "homeCode": "EQA", "awayCode": "EQB", "kickoffUtc": "2026-06-15T19:00:00Z", "stage": "Group" }
  ]
}
```

1. Iniciar sesión como administrador (`POST /auth/login`) y copiar el `token` de la respuesta.
2. Enviar el archivo:

```bash
cd backend
curl -X POST http://localhost:5231/admin/tournaments/import \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  --data-binary @samples/sample-tournament.json
```

La respuesta `201` incluye el id del torneo y la cantidad de equipos y partidos creados. También se puede importar desde Swagger UI pegando el contenido del archivo en el cuerpo de la solicitud.

Reglas del archivo:
- El código de equipo tiene exactamente 3 letras y no se repite. Los nombres de equipo tampoco se repiten.
- Los partidos solo pueden usar códigos de equipos del archivo, y un equipo no puede jugar contra sí mismo.
- `kickoffUtc` va en UTC con formato ISO 8601: sufijo `Z` u offset `+00:00`. Una fecha sin zona horaria o con otro offset se rechaza.
- `stage` es `Group` o `Knockout`.
- Si ya existe un equipo con el mismo código (por ejemplo, de otro torneo), se reutiliza y se ignora el nombre del archivo.
- La importación es todo o nada: si hay algún error, la API responde `400` con los mensajes por campo (ej. `matches[1].awayCode`) y no guarda nada. Un usuario que no es administrador recibe `403`.

### Ejecutar el frontend
Pendiente: se completa con el issue del esqueleto de Angular.
