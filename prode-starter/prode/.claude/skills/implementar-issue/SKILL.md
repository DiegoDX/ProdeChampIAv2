---
name: implementar-issue
description: Implementa un issue del repositorio Prode de principio a fin (rama, código, tests y pull request), y corrige el PR si el CI falla. Úsalo cuando el usuario pida implementar, resolver o trabajar un issue por su número, o arreglar un PR en rojo.
---

# Implementar un issue

Recibe el número del issue. Sigue estos pasos en orden.

1. Lee `CLAUDE.md` y respeta sus reglas de idioma, arquitectura, reglas del prode y forma de trabajo.
2. Lee el issue con `gh issue view <n>`. Los criterios de aceptación y la sección "Fuera de alcance" definen el alcance exacto. Comprueba que los issues de los que depende estén cerrados; si no, detente y avisa.
3. Antes de escribir código, explica el plan en máximo 8 líneas: qué archivos o proyectos vas a crear o tocar y por qué. Si hay una decisión de diseño con alternativas reales, di cuál eliges y por qué.
4. Parte de `main` actualizado y crea la rama `feature/<n>-<descripcion-corta>`.
5. Si el issue tiene "Tests requeridos", escribe primero esos tests (puedes seguir el skill `escribir-tests`).
6. Implementa solo lo que pide el issue. Si encuentras algo ajeno que conviene arreglar, no lo arregles: anótalo en el PR como sugerencia para otro issue.
7. Verifica antes de entregar:
   - Backend: `dotnet build` y `dotnet test`
   - Frontend: `ng build` y `ng test --watch=false`
   Si algo falla, corrige el código y vuelve a ejecutar hasta que todo pase.
   - Nunca borres, desactives ni debilites un test para que pase.
   - Si crees que el test está mal (no el código), no lo cambies por tu cuenta: explica en el PR por qué y espera la decisión del usuario.
   - Si después de 3 intentos sigue fallando, detente y explica el problema en lugar de seguir probando.
8. Haz commits pequeños con mensajes en inglés, en imperativo.
9. Sube la rama y abre el PR con `gh pr create`, usando la plantilla del repositorio. La descripción, en español, debe incluir `Closes #<n>`, qué se hizo, por qué se hizo así (decisiones y alternativas) y cómo probarlo.
10. No hagas merge. La revisión y el merge los hace el usuario.
11. Si el CI del PR falla (ahora o cuando el usuario lo indique), revisa el log con `gh pr checks <n>` y `gh run view <run-id> --log-failed`, corrige en la misma rama y sube el arreglo. Aplican las mismas reglas del paso 7.
