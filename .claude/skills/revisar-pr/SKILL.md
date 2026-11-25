---
name: revisar-pr
description: Revisa un pull request del proyecto Prode contra su issue y las reglas de CLAUDE.md. Úsalo cuando el usuario pida revisar o auditar un PR.
---

# Revisar un pull request

Recibe el número del PR. No modifiques código ni publiques comentarios en GitHub a menos que el usuario lo pida. Lo ideal es correr esta revisión en una sesión distinta de la que escribió el código.

1. Lee `CLAUDE.md`.
2. Obtén el PR, su estado de CI y el issue que cierra: `gh pr view <n>`, `gh pr checks <n>`, `gh pr diff <n>` y `gh issue view <issue>`.
3. Evalúa estos puntos:
   - **CI:** si está en rojo, el veredicto es "cambios necesarios", sin excepción.
   - **Criterios del issue:** cuáles se cumplen y cuáles no.
   - **Tests requeridos:** que existan todos los que pide el issue.
   - **Tests manipulados (bloqueante):** cualquier test borrado, desactivado (`Skip`, comentado) o con la aserción cambiada para que pase.
   - **Alcance:** cambios que no pertenecen al issue.
   - **Arquitectura:** dirección de las dependencias (Domain no referencia a nadie), sin lógica de negocio en controladores, uso de `TimeProvider`.
   - **Reglas del prode:** coinciden con `CLAUDE.md`.
   - **Idioma:** código en inglés; comentarios, documentación y mensajes al usuario en español.
   - **Seguridad:** sin secretos en el repo, contraseñas con hash, endpoints protegidos donde corresponde.
   - **Legibilidad:** nombres claros, sin código muerto.
4. Entrega el resultado así:
   - Veredicto: listo para merge, o cambios necesarios.
   - Problemas ordenados por gravedad (bloqueante, importante, menor), cada uno con archivo y línea.
   - Tres preguntas que un entrevistador técnico podría hacer sobre este PR, con una respuesta breve a cada una. Sirven para que el usuario entienda lo que está aprobando.
