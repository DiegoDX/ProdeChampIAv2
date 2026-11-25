---
name: actualizar-docs
description: Actualiza README, docs y CLAUDE.md del proyecto Prode después de que se hace merge de un PR. Úsalo cuando el usuario diga que aceptó un PR o pida poner al día la documentación.
---

# Actualizar la documentación

Recibe el número del PR que se acaba de aceptar (o revisa los últimos merges con `gh pr list --state merged --limit 5`).

1. Lee `CLAUDE.md`, `README.md` y el diff del PR con `gh pr diff <n>`.
2. Revisa si el PR cambió algo que la documentación deba reflejar:
   - Cómo ejecutar, configurar o probar el proyecto → `README.md`
   - Endpoints nuevos o modificados → sección de API del `README.md` (lista breve: método, ruta, para qué sirve, quién puede usarlo)
   - Decisiones de arquitectura o reglas del prode → `CLAUDE.md`
   - Despliegue → `docs/deployment.md`
3. Si no hay nada que actualizar, dilo y no cambies nada.
4. Si hay cambios, créalos en la rama `docs/after-pr-<n>`, con todo en español, y abre un PR corto con `gh pr create`. No hagas merge.
5. No reescribas secciones que siguen siendo correctas; cambia solo lo necesario.
