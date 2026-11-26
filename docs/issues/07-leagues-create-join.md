# Crear liga y unirse con código de invitación

## Objetivo
Que un usuario cree una liga para su grupo de amigos sobre un torneo y los invite con un código.

## Detalle técnico
Endpoints (usuarios autenticados):
- `POST /leagues` con `{ name, tournamentId }` → 201 con la liga y su `inviteCode`. El creador queda como dueño y como primer miembro.
- `POST /leagues/join` con `{ inviteCode }` → 200 con la liga
- `GET /leagues` → mis ligas (donde soy miembro)
- `GET /leagues/{id}` → detalle con la lista de miembros; solo para miembros (si no, 404 para no revelar que existe)

Reglas:
- Código de invitación de 8 caracteres en mayúsculas, sin caracteres ambiguos (sin 0, O, 1, I, L), aleatorio y único.
- La búsqueda por código no distingue mayúsculas de minúsculas.
- No se puede unir dos veces a la misma liga (409).
- No se puede crear una liga para un torneo `Finished`.
- Nombre de liga de 3 a 50 caracteres.

## Criterios de aceptación
- [ ] Los cuatro endpoints funcionan según lo descrito
- [ ] Un usuario que no es miembro no puede ver el detalle de la liga
- [ ] Mensajes de error en español

## Tests requeridos
- El generador de códigos solo usa caracteres permitidos y tiene longitud 8
- Crear liga deja al creador como miembro
- Unirse con código válido, con código inexistente (404) y dos veces (409)
- Crear liga para torneo finalizado → error

## Fuera de alcance
Expulsar miembros, salir de una liga, borrar ligas y regenerar el código.

## Depende de
Issue 6 (torneos y partidos).
