---
name: nuevo-issue
description: Convierte una idea corta del usuario en un issue completo del proyecto Prode y lo crea en GitHub. Úsalo cuando el usuario pida crear, redactar o proponer un issue nuevo.
---

# Crear un issue nuevo

Recibe una idea, aunque sea de una sola línea.

1. Lee `CLAUDE.md` y lista los issues existentes con `gh issue list --state all` para no duplicar trabajo y saber de cuáles depende.
2. Si la idea es ambigua en algo que cambia el alcance (una regla del prode, quién puede hacer qué), haz como máximo 2 preguntas antes de redactar. No preguntes lo que puedas decidir con las convenciones del proyecto.
3. Redacta el issue en español con estas secciones, igual que los de `docs/issues/`:
   - `## Objetivo`: qué se quiere lograr y por qué.
   - `## Detalle técnico`: endpoints, entidades o pantallas involucradas, con nombres en inglés.
   - `## Criterios de aceptación`: lista de casillas verificables.
   - `## Tests requeridos`: casos concretos, incluidos los límites.
   - `## Fuera de alcance`: lo que no se hace aquí.
   - `## Depende de`: issues previos necesarios.
4. Si el issue es demasiado grande para un solo PR (por ejemplo, más de unos 10 criterios o mezcla backend y frontend sin necesidad), propón dividirlo.
5. Muestra el borrador al usuario y espera su aprobación antes de crearlo.
6. Créalo con `gh issue create --title "<título>" --body-file <archivo temporal>` y devuelve el número y el enlace.
