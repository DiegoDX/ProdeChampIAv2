---
name: escribir-tests
description: Escribe los tests de un issue del proyecto Prode antes de implementar el código, a partir de sus criterios de aceptación y tests requeridos. Úsalo cuando el usuario pida los tests de un issue o al empezar una implementación con tests primero.
---

# Escribir los tests de un issue

Recibe el número del issue.

1. Lee `CLAUDE.md` y el issue con `gh issue view <n>`.
2. Convierte cada punto de "Tests requeridos" y cada criterio de aceptación verificable en uno o más tests. Agrega los casos límite que falten (valores en el borde, nulos, listas vacías, el instante exacto de un cierre).
3. Convenciones:
   - Backend: xUnit en `backend/tests`. Usa `[Theory]` con `[InlineData]` cuando varios casos prueban la misma regla.
   - Nombres en inglés con el patrón `Method_Scenario_ExpectedResult` (ej. `ScoreMatch_ExactResult_Returns5`).
   - Para fechas y horas, usa un `TimeProvider` falso; nunca la hora real.
   - Sin base de datos real en unit tests: usa implementaciones en memoria o dobles de prueba de las interfaces.
   - Un comentario breve en español solo cuando el porqué del caso no sea obvio.
   - Frontend: las pruebas del framework de Angular configurado en el proyecto.
4. Los tests deben compilar. Que fallen porque el código todavía no existe es lo esperado en este paso.
5. Entrega una lista de los tests creados, agrupados por regla, indicando cuáles cubren cada criterio del issue.
