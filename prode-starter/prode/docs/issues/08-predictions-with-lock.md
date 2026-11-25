# Pronósticos de partidos con cierre al comenzar el partido

## Objetivo
Que cada miembro pronostique el resultado de los partidos, pudiendo editarlo hasta que el partido comience.

## Detalle técnico
Endpoints (miembros de la liga):
- `PUT /leagues/{leagueId}/matches/{matchId}/prediction` con `{ homeGoals, awayGoals }` → crea o actualiza el pronóstico
- `GET /leagues/{leagueId}/predictions/me` → mis pronósticos en la liga, con los datos del partido
- `GET /leagues/{leagueId}/matches/{matchId}/predictions` → pronósticos de todos los miembros para ese partido, **solo después del inicio** (antes devuelve 403), para que nadie copie

Reglas:
- Cierre: si la hora actual (UTC) es igual o posterior a `KickoffUtc`, se rechaza con 409 y el mensaje "El partido ya comenzó; no se pueden cargar ni modificar pronósticos".
- La hora actual se obtiene con `TimeProvider` inyectado, nunca con `DateTime.UtcNow` directo, para poder probar el límite.
- Goles: enteros entre 0 y 20.
- El partido debe pertenecer al torneo de la liga.
- El usuario debe ser miembro de la liga (si no, 404).

## Criterios de aceptación
- [ ] Se puede crear y editar un pronóstico antes del inicio
- [ ] Se rechaza cualquier cambio desde el momento exacto del inicio
- [ ] Los pronósticos ajenos no se ven antes del inicio
- [ ] Mensajes de error en español

## Tests requeridos (con un `TimeProvider` falso)
- Un segundo antes del inicio → se acepta
- Justo en el inicio → se rechaza
- Después del inicio → se rechaza
- Editar un pronóstico existente lo actualiza, no lo duplica
- Goles negativos o mayores a 20 → error de validación
- Partido de otro torneo → error
- Ver pronósticos ajenos antes del inicio → 403; después → 200

## Fuera de alcance
Campeón y goleador, y cálculo de puntos.

## Depende de
Issue 7 (ligas).
