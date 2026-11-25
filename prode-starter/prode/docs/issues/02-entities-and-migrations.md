# Entidades del dominio y migración inicial

## Objetivo
Modelar los datos del prode y crear el esquema en SQL Server. Los nombres de entidades, propiedades, tablas y columnas van en inglés.

## Detalle técnico
Entidades en `Prode.Domain` (todas con `Id` entero salvo que se indique):

- **User**: `Email` (único), `DisplayName`, `PasswordHash`, `IsAdmin` (bool, por defecto false), `CreatedAtUtc`
- **Tournament**: `Name`, `Status` (enum: `Upcoming`, `InProgress`, `Finished`), `ChampionTeamId` (nullable, se completa al final)
- **Team**: `Name`, `Code` (3 letras, por ejemplo ARG), `TournamentId`
- **Player**: `FullName`, `TeamId` (para elegir goleador)
- **Match**: `TournamentId`, `HomeTeamId`, `AwayTeamId`, `KickoffUtc`, `Stage` (enum: `Group`, `Knockout`), `HomeGoals` (nullable), `AwayGoals` (nullable), `Status` (enum: `Scheduled`, `Finished`)
- **League**: `Name`, `TournamentId`, `InviteCode` (único), `OwnerUserId`, `CreatedAtUtc`
- **LeagueMember**: `LeagueId`, `UserId`, `JoinedAtUtc`. Clave primaria compuesta (`LeagueId`, `UserId`)
- **Prediction**: `LeagueId`, `UserId`, `MatchId`, `HomeGoals`, `AwayGoals`, `Points` (nullable, se calcula al cargar el resultado), `CreatedAtUtc`, `UpdatedAtUtc`
- **SpecialPrediction**: `LeagueId`, `UserId`, `ChampionTeamId`, `TopScorerPlayerId`, `Points` (nullable)

Configuración en `Prode.Infrastructure` con `IEntityTypeConfiguration<T>` (una clase por entidad, no todo en `OnModelCreating`).

Restricciones:
- Índice único en `User.Email` y en `League.InviteCode`
- Índice único en `Prediction` (`LeagueId`, `UserId`, `MatchId`): un solo pronóstico por usuario, liga y partido
- Índice único en `SpecialPrediction` (`LeagueId`, `UserId`)
- Borrado restrictivo en las relaciones (no borrar en cascada pronósticos al borrar equipos o partidos)
- Fechas siempre en UTC

## Criterios de aceptación
- [ ] Las 9 entidades existen en `Prode.Domain` sin atributos de EF Core
- [ ] Configuraciones separadas por entidad en `Prode.Infrastructure`
- [ ] Migración inicial que crea el esquema completo en una base vacía sin errores
- [ ] Las restricciones únicas listadas existen en la base de datos
- [ ] El README explica cómo aplicar las migraciones

## Tests requeridos
- No se exigen tests de base de datos en este issue. Si se agrega lógica en entidades (por ejemplo, validaciones en constructores), lleva su unit test.

## Fuera de alcance
Endpoints, lógica de puntos y datos de ejemplo.

## Depende de
Issue 1 (esqueleto de la solución).
