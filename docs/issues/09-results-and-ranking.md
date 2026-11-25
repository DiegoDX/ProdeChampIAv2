# Resultados reales y ranking de la liga

## Objetivo
Que el administrador cargue el resultado real de cada partido, que el sistema calcule los puntos y que cada liga tenga su ranking.

## Detalle técnico
Endpoints:
- `PUT /admin/matches/{matchId}/result` (política `AdminOnly`) con `{ homeGoals, awayGoals }`
  - Los goles son los del final del tiempo reglamentario más la prórroga; los penales no se cargan.
  - Marca el partido como `Finished` y calcula los `Points` de **todos** los pronósticos de ese partido, en todas las ligas, usando `PredictionScorer`.
  - Es idempotente: si se corrige un resultado cargado por error, se recalculan los puntos.
  - No se puede cargar el resultado de un partido que todavía no comenzó.
- `GET /leagues/{leagueId}/ranking` (miembros) → lista con `position`, `userId`, `displayName`, `totalPoints`, `exactHits`, `outcomeHits`

Reglas del ranking:
- `totalPoints` = puntos de partidos + puntos del pronóstico especial (campeón y goleador, cuando existan).
- `exactHits` = cantidad de pronósticos con 5 puntos; `outcomeHits` = cantidad con 3 puntos.
- Orden: más puntos; si empatan, más resultados exactos; si siguen empatados, más aciertos de ganador.
- Si siguen empatados, comparten posición y la siguiente se salta (1, 1, 3).
- Los miembros sin pronósticos aparecen con 0 puntos.
- El orden y las posiciones se calculan en una clase de dominio pura (`RankingCalculator`), separada del acceso a datos.

## Criterios de aceptación
- [ ] Cargar un resultado calcula los puntos de todos los pronósticos de ese partido
- [ ] Corregir un resultado recalcula correctamente
- [ ] El ranking respeta el orden y los desempates descritos
- [ ] Un usuario no administrador recibe 403 al cargar resultados

## Tests requeridos
- `RankingCalculator`: orden por puntos, desempate por exactos, desempate por ganador, posición compartida (1, 1, 3), miembro sin pronósticos
- Cargar resultado asigna puntos correctos
- Corregir resultado recalcula
- Resultado de un partido no comenzado → error

## Fuera de alcance
Resultados automáticos desde una API externa y notificaciones.

## Depende de
Issue 8 (pronósticos).
