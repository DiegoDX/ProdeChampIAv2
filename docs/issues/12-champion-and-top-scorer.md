# Pronóstico de campeón y goleador

## Objetivo
Completar las reglas del prode: cada miembro elige campeón y goleador del torneo, y suman 10 puntos cada uno.

## Detalle técnico
Backend:
- `PUT /leagues/{leagueId}/special-prediction` con `{ championTeamId, topScorerPlayerId }` (miembros)
- `GET /leagues/{leagueId}/special-prediction/me`
- Cierre: no se puede crear ni modificar desde el inicio del **primer partido del torneo** (mismo patrón con `TimeProvider` que los pronósticos de partidos).
- Los pronósticos especiales ajenos se ven recién después de ese cierre.
- El equipo y el jugador deben pertenecer al torneo de la liga.
- `PUT /admin/tournaments/{id}/final-result` (política `AdminOnly`) con `{ championTeamId, topScorerPlayerIds: [] }`:
  - Acepta varios goleadores por si terminan empatados (todos cuentan como acierto).
  - Marca el torneo como `Finished` y calcula los puntos de todos los pronósticos especiales con `PredictionScorer.ScoreSpecial`.
  - Es idempotente (se puede corregir).
- Nueva tabla `TournamentTopScorer` (`TournamentId`, `PlayerId`) con su migración, para guardar uno o varios goleadores reales.
- El ranking suma estos puntos al total.

Frontend:
- En el detalle de liga, una sección "Campeón y goleador" con un selector de equipo y otro de jugador (filtrado por equipo), en solo lectura después del cierre.

## Criterios de aceptación
- [ ] Un miembro puede elegir y cambiar campeón y goleador antes del primer partido
- [ ] Se rechaza desde el inicio del primer partido
- [ ] Al cargar el resultado final, los puntos se reflejan en el ranking
- [ ] Con dos goleadores empatados, elegir cualquiera de los dos suma 10

## Tests requeridos
- Cierre en el límite exacto del primer partido
- Equipo o jugador de otro torneo → error
- Cálculo con goleadores empatados
- Corrección del resultado final recalcula

## Fuera de alcance
Otros pronósticos especiales (subcampeón, valla menos vencida, etc.).

## Depende de
Issue 11 (pantallas de Angular).
