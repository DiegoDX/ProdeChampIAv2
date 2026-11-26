# Torneos, equipos y partidos (carga inicial)

## Objetivo
Tener torneos con su calendario de partidos cargados. Sin esto no hay nada que pronosticar.

## Detalle técnico
Carga por archivo JSON, a través de un endpoint de administrador:
- `POST /admin/tournaments/import` (política `AdminOnly`) recibe un JSON con este formato:

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

- La importación es transaccional: si algo es inválido, no se guarda nada.
- Validaciones: códigos de equipo existentes en el archivo, un equipo no juega contra sí mismo, fechas en UTC, sin equipos duplicados.
- Incluir en el repo un archivo de ejemplo pequeño en `backend/samples/sample-tournament.json` (4 equipos, 6 partidos, datos ficticios) para pruebas y desarrollo.

Endpoints de lectura (cualquier usuario autenticado):
- `GET /tournaments` → lista de torneos
- `GET /tournaments/{id}/matches` → partidos ordenados por fecha, con filtro opcional `?status=Scheduled|Finished`

## Criterios de aceptación
- [ ] Un administrador puede importar el archivo de ejemplo y ver los datos con los endpoints de lectura
- [ ] Un usuario no administrador recibe 403 al importar
- [ ] Un archivo inválido devuelve 400 con mensajes claros en español y no guarda nada
- [ ] El README explica cómo importar un torneo

## Tests requeridos
- Validación del JSON: código de equipo inexistente, equipo contra sí mismo, equipo duplicado
- Importación válida crea la cantidad correcta de equipos y partidos (puede probarse en la capa Application con repositorios simulados)

## Fuera de alcance
Editar o borrar torneos, y obtener calendarios de una API externa.

## Depende de
Issue 5 (autenticación).
