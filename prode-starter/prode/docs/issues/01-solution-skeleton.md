# Esqueleto de la solución .NET y conexión a SQL Server

## Objetivo
Crear la base del backend con Clean Architecture, lista para construir encima. Es el cimiento de todo el proyecto: si las dependencias quedan mal aquí, se arrastran en todos los issues siguientes.

## Detalle técnico
- Carpeta `/backend` con una solución `Prode.sln` y estos proyectos:
  - `src/Prode.Domain` (biblioteca de clases): entidades y reglas puras. Sin referencias a otros proyectos ni a paquetes de infraestructura.
  - `src/Prode.Application` (biblioteca de clases): casos de uso e interfaces (por ejemplo, repositorios). Referencia solo a `Domain`.
  - `src/Prode.Infrastructure` (biblioteca de clases): EF Core, `ProdeDbContext`, implementación de las interfaces. Referencia a `Application`.
  - `src/Prode.Api` (ASP.NET Core Web API): configuración, inyección de dependencias y endpoints. Referencia a `Application` e `Infrastructure`.
  - `tests/Prode.UnitTests` (xUnit): referencia a `Domain` y `Application`.
- Versión de .NET: la LTS más reciente disponible.
- EF Core con el proveedor de SQL Server. La cadena de conexión se lee de `ConnectionStrings:Default`.
- `appsettings.json` incluye la clave con un valor vacío o de ejemplo; el valor real va en *user-secrets* o en `appsettings.Development.json` (ignorado por git).
- Swagger/OpenAPI habilitado en desarrollo.
- Endpoint `GET /health` que responde 200 con un cuerpo simple (por ejemplo `{ "status": "ok" }`).

## Criterios de aceptación
- [ ] `dotnet build` en `/backend` compila sin errores
- [ ] `dotnet test` se ejecuta correctamente (aunque haya un solo test)
- [ ] `Prode.Domain` no tiene referencias a otros proyectos ni a EF Core
- [ ] `Prode.Application` solo referencia a `Prode.Domain`
- [ ] `GET /health` responde 200
- [ ] Ninguna cadena de conexión real queda en el repositorio
- [ ] El README tiene una sección "Ejecutar el backend": requisitos, configurar la cadena de conexión con user-secrets y `dotnet run`

## Tests requeridos
- Un test simple en `Prode.UnitTests` que confirme que el proyecto de tests funciona (se reemplaza con tests reales en el issue de la calculadora).

## Fuera de alcance
Entidades, migraciones, autenticación y CI.

## Depende de
Nada. Es el primer issue.
